"""
把夜城娱乐会所V1 (03-57-07) 替换为 NeonTower base 图。
- 备份当前 base (霓虹赌场版)
- 拷贝目标图
- 浅色棋盘格背景转透明 (RGB 都 >= 220 且差 < 12 的像素)
- 去除右下角 "由AI生成" 水印 (y∈[660,710], x∈[380,1280])
"""
from PIL import Image
import numpy as np
import shutil
from pathlib import Path

ROOT = Path(r"E:\测试1号\SBPK")
SRC = ROOT / "design" / "art" / "generated" / "2D_16_bit_pixel_art__1280x720__2026-07-22T03-57-07.png"
DST = ROOT / "Assets" / "Art" / "Buildings" / "NeonTower" / "Sprites" / "bldg_neon_tower_base.png"
BAK = ROOT / "Assets" / "Art" / "Buildings" / "NeonTower" / "Sprites" / "bldg_neon_tower_base_霓虹赌场.bak"

def is_background(rgb):
    r, g, b = int(rgb[0]), int(rgb[1]), int(rgb[2])
    # 浅灰白棋盘格: 三通道都偏高, 互相差很小
    if r >= 220 and g >= 220 and b >= 220 and max(r, g, b) - min(r, g, b) < 12:
        return True
    return False

def main():
    # 1. 备份当前 base
    if not BAK.exists():
        shutil.copy2(DST, BAK)
        print(f"[备份] {DST.name} -> {BAK.name}  ({DST.stat().st_size//1024} KB)")

    # 2. 拷贝目标图为 base
    shutil.copy2(SRC, DST)
    print(f"[替换] base 已是 03-57-07 夜城娱乐会所V1 ({DST.stat().st_size//1024} KB)")

    # 3. 加载并转 RGBA
    img = Image.open(DST).convert("RGB")
    arr = np.array(img)
    H, W, _ = arr.shape
    print(f"  尺寸: {W}x{H}")

    # 4. 浅色背景 -> 透明
    r, g, b = arr[:,:,0], arr[:,:,1], arr[:,:,2]
    bg_mask = (r >= 220) & (g >= 220) & (b >= 220) & \
              (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b) < 12)
    bg_pct = bg_mask.sum() / bg_mask.size * 100
    print(f"  浅色背景像素: {bg_pct:.1f}%")

    # 5. 去水印区域: y∈[660,710], x∈[380,1280]
    # 先尝试用周围非背景像素上采样平均填充; 简单起见, 把水印区域当成"如果是浅色背景就变透明, 否则保留"
    # (上一轮水印也是浅灰色, 会被上一步 bg_mask 一并处理)
    # 额外保险: 把这个区域里残留的浅色像素(alpha 待定区域) 强制透明
    y0, y1, x0, x1 = 660, 710, 380, 1280
    y1 = min(y1, H); x1 = min(x1, W)
    region_mask = np.zeros_like(bg_mask)
    region_mask[y0:y1, x0:x1] = True
    extra = region_mask & bg_mask
    print(f"  水印区域额外透明像素: {extra.sum()}")

    # 6. 应用透明
    rgba = np.zeros((H, W, 4), dtype=np.uint8)
    rgba[:, :, :3] = arr
    rgba[:, :, 3] = np.where(bg_mask, 0, 255)
    # 水印区域额外 (确保) -> 透明
    rgba[:, :, 3] = np.where(region_mask & bg_mask, 0, rgba[:, :, 3])

    # 7. 半透明边缘羽化: 紧邻背景的内部像素给一点过渡
    # 简化: 不做边缘羽化, 留给Unity Sprite
    Image.fromarray(rgba, "RGBA").save(DST, optimize=True)
    print(f"[保存] {DST}  ({DST.stat().st_size//1024} KB)")
    print(f"  最终透明比例: {(rgba[:,:,3]==0).sum()/rgba[:,:,3].size*100:.1f}%")

if __name__ == "__main__":
    main()
