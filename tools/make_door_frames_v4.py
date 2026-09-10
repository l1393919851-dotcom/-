"""
卷帘门序列帧重做 + base 门区域挖空。

设计:
  Canvas 72x81
  - y=0-5  : coil 空间 (6px, 卷起的鼓包)
  - y=6-13 : 顶部框架 (8px, 门框, 始终可见)
  - y=14-19: 暗缝 (6px, 门框和slat之间的空隙, 始终透明)
  - y=20-80: slat (61px, 从底一节节消失)
  Pivot: 底中 (0.5, 0)
  Pivot 对应 base 像素 (560, 699)
  Canvas top 对应 base 像素 y=619 (coil 区域在门上方)
"""
from PIL import Image
import numpy as np
import os

src = 'design/art/generated/2D_16_bit_pixel_art__1280x720__2026-07-22T03-57-07.png'
img = Image.open(src).convert('RGB')
arr = np.array(img)

# 抠门 (完整: y=625-704, 80px), 但只取门主体 (y=625-699, 75px)
# base 像素坐标: 门在 x=524-596 (72px), y=625-699 (75px)
# 门结构:
#   base y=625-632: top frame (8px)
#   base y=633-638: dark gap (6px)
#   base y=639-699: slat (61px)
DOOR_X0, DOOR_X1 = 524, 596  # 72px 宽
DOOR_Y0, DOOR_Y1 = 625, 700  # 75px 高
door = arr[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1].copy()
DOOR_H, DOOR_W = door.shape[:2]
print(f'门抠出: {DOOR_W}x{DOOR_H}')

# Canvas 设计
COIL_MAX = 6
TOP_FRAME = 8
DARK_GAP = 6
SLAT_MAX = 61
CANVAS_W = DOOR_W  # 72
CANVAS_H = COIL_MAX + TOP_FRAME + DARK_GAP + SLAT_MAX  # 6+8+6+61=81
print(f'Canvas: {CANVAS_W}x{CANVAS_H}')

# 8 帧: 0(全闭) -> 7(全开)
N_FRAMES = 8

# 删除旧帧
frames_dir = 'Assets/Art/Buildings/NeonTower/Sprites/Frames'
os.makedirs(frames_dir, exist_ok=True)
for f in os.listdir(frames_dir):
    if f.endswith('.png'):
        os.remove(f'{frames_dir}/{f}')

# 门抠图 RGBA 化 (排除浅色背景)
door_rgba = np.zeros((DOOR_H, DOOR_W, 4), dtype=np.uint8)
door_rgba[:,:,:3] = door
r, g, b = door[:,:,0], door[:,:,1], door[:,:,2]
light = (r >= 220) & (g >= 220) & (b >= 220) & \
        (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b) < 12)
door_alpha = np.where(light, 0, 255).astype(np.uint8)
door_rgba[:,:,3] = door_alpha

print(f'\n生成 {N_FRAMES} 帧:')
for i in range(N_FRAMES):
    t = i / (N_FRAMES - 1)  # 0..1
    # 卷起的 slat 像素数
    rolled = int(round(t * SLAT_MAX))
    # 可见 slat 像素数
    visible = SLAT_MAX - rolled
    # 鼓包高度 (随 rolled 线性增加)
    coil_h = int(round(t * COIL_MAX))

    canvas = np.zeros((CANVAS_H, CANVAS_W, 3), dtype=np.uint8)
    alpha = np.zeros((CANVAS_H, CANVAS_W), dtype=np.uint8)

    # 1) 鼓包 (y=0..coil_h)
    if coil_h > 0 and rolled > 0:
        # 从门底部 (slat 区域) 取 rolled 像素
        # slat 在 door 坐标: y=14-74 (TOP_FRAME=8, DARK_GAP=6, SLAT=61)
        # 但 slat 是从 door y=14 开始的 61px
        # 卷起的部分: 底部 rolled 像素 = door y=(14+61-rolled) 到 (14+61)
        slat_start_in_door = 14  # 第一个 slat 行
        slat_end_in_door = 14 + SLAT_MAX  # 75 (slat 结束)
        # 卷起的部分: slat 末尾 rolled 像素
        rolled_source = door_rgba[slat_end_in_door - rolled:slat_end_in_door, :, :3]
        # 压缩到 coil_h 像素
        coil_canvas = np.zeros((coil_h, CANVAS_W, 3), dtype=np.uint8)
        for cy in range(coil_h):
            sy_start = int(cy * rolled / coil_h)
            sy_end = int((cy + 1) * rolled / coil_h)
            sy_end = max(sy_end, sy_start + 1)
            region = rolled_source[sy_start:sy_end, :]
            coil_canvas[cy, :] = region.mean(axis=0).astype(np.uint8)
        # 鼓包变深 (阴影感)
        coil_canvas = (coil_canvas * 0.45).astype(np.uint8)
        # 加暗线 (slat 痕迹) - 每 2 像素一条
        for cy in range(1, coil_h, 2):
            coil_canvas[cy, :] = (coil_canvas[cy, :] * 0.5).astype(np.uint8)
        # 加中间高光 (圆鼓包)
        if coil_h >= 3:
            mid = coil_h // 2
            coil_canvas[mid, :] = np.clip(coil_canvas[mid, :] * 1.4, 0, 255).astype(np.uint8)
        canvas[0:coil_h, :] = coil_canvas
        alpha[0:coil_h, :] = 255

    # 2) 顶部框架 (固定 y=COIL_MAX..COIL_MAX+TOP_FRAME)
    top_frame = door_rgba[0:TOP_FRAME, :, :3]
    canvas[COIL_MAX:COIL_MAX+TOP_FRAME, :] = top_frame
    alpha[COIL_MAX:COIL_MAX+TOP_FRAME, :] = 255

    # 3) 暗缝 (固定 y=COIL_MAX+TOP_FRAME..COIL_MAX+TOP_FRAME+DARK_GAP)
    # 保持透明

    # 4) 可见 slat (从 y=COIL_MAX+TOP_FRAME+DARK_GAP 开始)
    if visible > 0:
        # slat 从 door y=14 开始
        visible_slat = door_rgba[14:14+visible, :, :3]
        start_y = COIL_MAX + TOP_FRAME + DARK_GAP
        canvas[start_y:start_y+visible, :] = visible_slat
        alpha[start_y:start_y+visible, :] = 255

    # 合并 alpha
    rgba = np.zeros((CANVAS_H, CANVAS_W, 4), dtype=np.uint8)
    rgba[:,:,:3] = canvas
    rgba[:,:,3] = alpha

    out_path = f'{frames_dir}/rollingdoor_{i:02d}.png'
    Image.fromarray(rgba, 'RGBA').save(out_path, optimize=True)
    print(f'  帧{i}: coil={coil_h}px, 框架=8px, 暗缝=6px, 可见slat={visible}px, 卷起={rolled}px')

# ===== Base 处理: 背景抠透明 + 门洞涂黑 =====
# 把原图浅灰/白色背景抠成透明, 留下建筑; 门洞涂黑露出"室内"
# 序列帧播完时, 卷帘门消失, 露出 base 上预涂的纯黑门洞
print(f'\n处理 base: 背景抠透明 + 门洞涂黑 ({DOOR_X0}-{DOOR_X1}, {DOOR_Y0}-{DOOR_Y1-1})')
img2 = Image.open('Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png').convert('RGB')
arr2 = np.array(img2).astype(np.float32)

r2, g2, b2 = arr2[:,:,0], arr2[:,:,1], arr2[:,:,2]
avg2 = (r2 + g2 + b2) / 3.0
sat2 = np.maximum(np.maximum(r2, g2), b2) - np.minimum(np.minimum(r2, g2), b2)

# 识别浅灰背景 + 右下角水印 → 透明
bg_mask2 = (avg2 > 200) & (sat2 < 20)
watermark_mask2 = np.zeros_like(bg_mask2, dtype=bool)
watermark_mask2[680:720, 1050:1280] = True
watermark_mask2 &= (avg2 > 180)
combined_mask2 = bg_mask2 | watermark_mask2

# 门洞涂纯黑 (RGB=0)
arr2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1] = [0, 0, 0]

# 输出 RGBA: 背景透明 (alpha=0), 建筑+门洞不透明 (alpha=255)
out_rgb = arr2.astype(np.uint8)
rgba2 = np.zeros((out_rgb.shape[0], out_rgb.shape[1], 4), dtype=np.uint8)
rgba2[:, :, :3] = out_rgb
rgba2[:, :, 3] = np.where(combined_mask2, 0, 255)

Image.fromarray(rgba2, 'RGBA').save('Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png', optimize=True)
print(f'Base 已处理: 透明像素 {combined_mask2.sum()} 个, 门洞涂黑 72x75, 输出 RGBA')

# ===== 拼预览 (黑底 + 帧号) =====
print(f'\n生成预览:')
frames = []
for i in range(N_FRAMES):
    img = Image.open(f'{frames_dir}/rollingdoor_{i:02d}.png').convert('RGBA')
    frames.append(np.array(img))

from PIL import ImageDraw
# 黑底横排
bg = np.zeros((CANVAS_H + 28, CANVAS_W * N_FRAMES, 4), dtype=np.uint8)
bg[28:, :, 0] = 20
bg[28:, :, 1] = 22
bg[28:, :, 2] = 35
bg[28:, :, 3] = 255
for i, f in enumerate(frames):
    a = f[:,:,3:4] / 255.0
    bg[28:, i*CANVAS_W:(i+1)*CANVAS_W, :3] = (
        f[:,:,:3] * a + bg[28:, i*CANVAS_W:(i+1)*CANVAS_W, :3] * (1-a)
    ).astype(np.uint8)
    bg[28:, i*CANVAS_W:(i+1)*CANVAS_W, 3] = 255

img_pil = Image.fromarray(bg, 'RGBA')
draw = ImageDraw.Draw(img_pil)
for i in range(N_FRAMES):
    draw.text((i*CANVAS_W + CANVAS_W//2 - 5, 6), f'#{i}', fill=(255, 200, 100, 255))
img_pil.save('tools/_frames_final_strip.png', optimize=True)
print(f'  tools/_frames_final_strip.png (8帧横排)')

# 在 base 上叠加预览 (把序列帧画在 base 的门位置)
# 用 4 帧 (0, 2, 4, 6) 模拟游戏里不同时间点
test_frames = [0, 2, 4, 6]
preview_base = arr2.copy()
xs = [400, 600, 800, 1000]
for px, fi in zip(xs, test_frames):
    frame = frames[fi]
    # 放置位置: base 像素 x=px, y=DOOR_Y0-COIL_MAX (因为 frame 顶部有 COIL_MAX 的 coil 空间)
    # 实际: frame 顶 (y=0) 对应 base y = 625 - COIL_MAX = 619
    py = 619  # frame y=0 在 base 的位置
    y_end = min(py + CANVAS_H, preview_base.shape[0])
    x_end = min(px + CANVAS_W, preview_base.shape[1])
    fh = y_end - py
    fw = x_end - px
    if fh > 0 and fw > 0:
        a = frame[:fh, :fw, 3:4] / 255.0
        preview_base[py:y_end, px:x_end, :3] = (
            frame[:fh, :fw, :3] * a + preview_base[py:y_end, px:x_end, :3] * (1-a)
        ).astype(np.uint8)
        preview_base[py:y_end, px:x_end, 3] = 255

Image.fromarray(preview_base, 'RGBA').save('tools/_preview_on_base_v2.png', optimize=True)
print(f'  tools/_preview_on_base_v2.png (帧 0/2/4/6 叠在 base 旁边)')
print('完成!')
