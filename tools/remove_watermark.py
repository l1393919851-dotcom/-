#!/usr/bin/env python3
"""
移除 AI 生成图片右下角的水印（"图片由AI生成"等白色半透明文字）。
专为像素风格/2D游戏资产设计：检测水印区域并用上方非水印像素填充，
避免破坏画面上方的关键内容。

用法:
    python remove_watermark.py <图片路径> [图片路径...]

示例:
    python remove_watermark.py sprite.png
    python remove_watermark.py *.png                    # 批处理
    python remove_watermark.py --inplace *.png           # 直接覆盖原文件
"""

import argparse
import os
import sys

try:
    from PIL import Image
    import numpy as np
except ImportError:
    print("❌ 需要安装 Pillow 和 numpy。")
    print("   pip install Pillow numpy")
    sys.exit(1)


def detect_watermark_region(arr: np.ndarray, padding: int = 4) -> tuple:
    """
    自动检测图片右下角的水印区域。
    
    策略：从右下角向上扫描，找到连续 N 行亮度显著高于背景的行。
    返回 (y_start, y_end, x_start, x_end) 或 None。
    """
    h, w = arr.shape[:2]
    
    # 只在右下 1/4 区域检测
    search_x = w * 3 // 4
    search_y = h * 3 // 4
    
    roi = arr[search_y:, search_x:].astype(np.float32)
    row_brightness = roi.mean(axis=(1, 2))  # 每行平均亮度
    
    # 计算"背景亮度"：取最后5行的中位数
    if len(row_brightness) > 10:
        bg_bright = np.median(row_brightness[-5:])
    else:
        bg_bright = row_brightness[-1] if len(row_brightness) > 0 else 0
    
    # 找亮度显著高于背景的连续行区域
    threshold = bg_bright + 3.0  # 超过背景3个亮度等级
    above_bg = row_brightness > threshold
    
    # 找最长连续 True 段
    max_len = 0
    max_start = 0
    current_len = 0
    current_start = 0
    
    for i, v in enumerate(above_bg):
        if v:
            if current_len == 0:
                current_start = i
            current_len += 1
            if current_len > max_len:
                max_len = current_len
                max_start = current_start
        else:
            current_len = 0
    
    if max_len < 3:  # 至少3行才认为是水印
        return None
    
    y_start = search_y + max_start - padding
    y_end = search_y + max_start + max_len + padding
    
    # 在水印y范围内，逐列检测x起始
    water_roi = arr[y_start:y_end, search_x:]
    col_brightness = water_roi.mean(axis=0)
    col_brightness_avg = col_brightness.mean(axis=1) if col_brightness.ndim > 1 else col_brightness
    x_threshold = np.median(col_brightness_avg) + 3.0
    
    bright_cols = np.where(col_brightness_avg > x_threshold)[0]
    if len(bright_cols) == 0:
        # 回退到检测整个右下
        x_start = search_x - padding
        x_end = w - 1
    else:
        x_start = search_x + max(0, bright_cols[0] - padding)
        x_end = search_x + min(w - search_x, bright_cols[-1] + padding)
    
    return (
        max(search_y, y_start),
        min(h, y_end),
        max(0, x_start - padding),
        min(w, x_end + padding),
    )


def remove_watermark(
    image_path: str,
    output_path: str = None,
    inplace: bool = False,
    method: str = "auto",
    region: tuple = None,
    fill_above_rows: int = 8,
    threshold: float = 20.0,
):
    """
    移除图片右下角水印。
    
    Args:
        image_path: 输入图片路径
        output_path: 输出路径（默认在原文件名后加 _nowm）
        inplace: 直接覆盖原文件（优先级高于 output_path）
        method: "auto" 自动检测水印区域，或 "crop" 裁剪，或 "none" 只返回区域
        region: (y1,y2,x1,x2) 手动指定水印区域
        fill_above_rows: 从水印区域上方多少行采样填充
        threshold: 判定为水印像素的亮度阈值（RGB和）
    """
    img = Image.open(image_path).convert("RGB")
    arr = np.array(img, dtype=np.uint8)
    h, w = arr.shape[:2]
    
    # ---- 检测或使用指定水印区域 ----
    if region:
        y1, y2, x1, x2 = region
    else:
        detected = detect_watermark_region(arr)
        if detected is None:
            print(f"  ⚠ 未自动检测到水印区域，跳过: {os.path.basename(image_path)}")
            return False
        y1, y2, x1, x2 = detected
    
    # 边界保护
    y1 = max(0, y1)
    y2 = min(h, y2)
    x1 = max(0, x1)
    x2 = min(w, x2)
    
    if y2 <= y1 or x2 <= x1:
        print(f"  ⚠ 水印区域无效: ({y1}-{y2}, {x1}-{x2})")
        return False
    
    # ---- 填充水印区域 ----
    # 取水印区域正上面的非水印像素
    fill_y_start = max(0, y1 - fill_above_rows)
    fill_y_end = y1
    
    # 上方区域可能是"纯色背景"或"画面内容"
    # 策略：逐列处理，每列取上方最近非水印行
    for col in range(x1, x2):
        # 取上方 fill_above_rows 行的平均值
        above = arr[fill_y_start:fill_y_end, col].astype(np.float32).mean(axis=0).astype(np.uint8)
        
        for row in range(y1, y2):
            pixel = arr[row, col].astype(np.float32)
            
            # 判断是否为水印像素：亮度显著高于上方采样
            brightness_diff = pixel.mean() - above.mean()
            
            if brightness_diff > threshold / 3:
                # 水印像素 → 用上方采样填充
                arr[row, col] = above
            else:
                # 非水印像素，保持原样，但更新此列上方参考值
                above = arr[row, col].astype(np.uint8)
    
    # ---- 保存 ----
    result = Image.fromarray(arr)
    
    if inplace:
        save_path = image_path
    elif output_path:
        save_path = output_path
    else:
        base, ext = os.path.splitext(image_path)
        save_path = f"{base}_nowm{ext}"
    
    result.save(save_path)
    
    print(f"  ✅ {os.path.basename(image_path)} → {os.path.basename(save_path)}")
    print(f"     水印区域: y=[{y1}-{y2}) x=[{x1}-{x2}), 共 {(y2-y1)*(x2-x1)}px")
    return True


def main():
    parser = argparse.ArgumentParser(description="移除 AI 生成图片右下角水印")
    parser.add_argument("files", nargs="+", help="图片文件路径（支持通配符）")
    parser.add_argument("--inplace", "-i", action="store_true", help="直接覆盖原文件")
    parser.add_argument("--output-dir", "-o", help="输出目录（默认与原文件同目录）")
    parser.add_argument("--region", type=str, default=None,
                        help="手动指定水印区域: y1,y2,x1,x2 (如 668,701,380,1280)")
    parser.add_argument("--threshold", type=float, default=20.0,
                        help="水印检测亮度阈值（默认20.0）")
    parser.add_argument("--fill-rows", type=int, default=8,
                        help="水印上方采样行数（默认8）")
    parser.add_argument("--detect-only", action="store_true",
                        help="只检测不处理，显示检测到的区域")
    
    args = parser.parse_args()
    
    region = None
    if args.region:
        parts = [int(x) for x in args.region.replace(",", " ").split()]
        if len(parts) == 4:
            region = tuple(parts)
    
    success_count = 0
    for filepath in args.files:
        if not os.path.isfile(filepath):
            print(f"  ⚠ 文件不存在: {filepath}")
            continue
        
        ext = os.path.splitext(filepath)[1].lower()
        if ext not in (".png", ".jpg", ".jpeg", ".bmp", ".tga", ".webp"):
            print(f"  ⚠ 不支持的格式: {filepath}")
            continue
        
        if args.detect_only:
            img = Image.open(filepath).convert("RGB")
            arr = np.array(img)
            detected = detect_watermark_region(arr)
            if detected:
                y1, y2, x1, x2 = detected
                print(f"  📐 {os.path.basename(filepath)}: y=[{y1}-{y2}] x=[{x1}-{x2}]")
            else:
                print(f"  ❌ {os.path.basename(filepath)}: 未检测到水印区域")
            continue
        
        ok = remove_watermark(
            filepath,
            inplace=args.inplace,
            region=region,
            fill_above_rows=args.fill_rows,
            threshold=args.threshold,
        )
        if ok:
            success_count += 1
    
    print(f"\n=== 完成: {success_count}/{len([f for f in args.files if os.path.isfile(f)])} 张已处理 ===")


if __name__ == "__main__":
    main()
