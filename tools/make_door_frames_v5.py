"""
卷帘门序列帧 v5: 修复覆盖不全问题。

设计变更:
  - Canvas 高度 = 门洞高度 = 75px (不再让 coil 鼓包高出门洞)
  - 每帧都完整覆盖 72x75 区域
  - dark_gap 改为不透明 (作为门本身的阴影缝隙)
  - frame 0: 完整覆盖门洞 (top_frame + dark_gap + slat)
  - frame 7: 只保留顶部卷起的 coil + frame + gap, 下方透明露出黑门洞
  - Pivot: 底中 (0.5, 0), 底部对齐门洞底

Canvas 72x75:
  - y=0..coil_h-1   : coil (卷起的鼓包, 随进度出现)
  - y=coil_h..coil_h+TOP_FRAME-1 : 顶部框架 (固定)
  - y=coil_h+TOP_FRAME..coil_h+TOP_FRAME+DARK_GAP-1 : 暗缝/阴影 (固定)
  - y=coil_h+TOP_FRAME+DARK_GAP..CANVAS_H-1 : 可见 slat (从底向上减少)
"""
from PIL import Image
import numpy as np
import os

src = 'design/art/generated/2D_16_bit_pixel_art__1280x720__2026-07-22T03-57-07.png'
img = Image.open(src).convert('RGB')
arr = np.array(img)

# 门在 base 像素坐标: x=524-596 (72px), y=625-699 (75px)
DOOR_X0, DOOR_X1 = 524, 596
DOOR_Y0, DOOR_Y1 = 625, 700

door = arr[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1].copy()
DOOR_H, DOOR_W = door.shape[:2]
print(f'门抠出: {DOOR_W}x{DOOR_H}')

# Canvas 设计
TOP_FRAME = 8
DARK_GAP = 6
SLAT_MAX = DOOR_H - TOP_FRAME - DARK_GAP  # 75 - 8 - 6 = 61
COIL_MAX = 6
CANVAS_W = DOOR_W   # 72
CANVAS_H = DOOR_H   # 75
print(f'Canvas: {CANVAS_W}x{CANVAS_H}, slat_max={SLAT_MAX}, coil_max={COIL_MAX}')

N_FRAMES = 8

frames_dir = 'Assets/Art/Buildings/NeonTower/Sprites/Frames'
os.makedirs(frames_dir, exist_ok=True)
for f in os.listdir(frames_dir):
    if f.endswith('.png'):
        os.remove(f'{frames_dir}/{f}')

# 门抠图 RGBA 化 (排除浅色背景)
door_rgba = np.zeros((DOOR_H, DOOR_W, 4), dtype=np.uint8)
door_rgba[:, :, :3] = door
r, g, b = door[:, :, 0], door[:, :, 1], door[:, :, 2]
light = (r >= 220) & (g >= 220) & (b >= 220) & \
        (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b) < 12)
door_alpha = np.where(light, 0, 255).astype(np.uint8)
door_rgba[:, :, 3] = door_alpha

print(f'\n生成 {N_FRAMES} 帧:')
for i in range(N_FRAMES):
    t = i / (N_FRAMES - 1)  # 0..1

    rolled = int(round(t * SLAT_MAX))      # 已卷起的 slat 像素数
    visible = SLAT_MAX - rolled             # 仍可见的 slat 像素数
    coil_h = int(round(t * COIL_MAX))       # 顶部鼓包高度

    canvas = np.zeros((CANVAS_H, CANVAS_W, 3), dtype=np.uint8)
    alpha = np.zeros((CANVAS_H, CANVAS_W), dtype=np.uint8)

    # 1) 鼓包 (y=0..coil_h-1)
    if coil_h > 0 and rolled > 0:
        slat_end_in_door = TOP_FRAME + DARK_GAP + SLAT_MAX  # 8 + 6 + 61 = 75
        rolled_source = door_rgba[slat_end_in_door - rolled:slat_end_in_door, :, :3]
        coil_canvas = np.zeros((coil_h, CANVAS_W, 3), dtype=np.uint8)
        for cy in range(coil_h):
            sy_start = int(cy * rolled / coil_h)
            sy_end = int((cy + 1) * rolled / coil_h)
            sy_end = max(sy_end, sy_start + 1)
            region = rolled_source[sy_start:sy_end, :]
            coil_canvas[cy, :] = region.mean(axis=0).astype(np.uint8)
        # 鼓包变深
        coil_canvas = (coil_canvas * 0.45).astype(np.uint8)
        # 加暗线
        for cy in range(1, coil_h, 2):
            coil_canvas[cy, :] = (coil_canvas[cy, :] * 0.5).astype(np.uint8)
        # 中间高光
        if coil_h >= 3:
            mid = coil_h // 2
            coil_canvas[mid, :] = np.clip(coil_canvas[mid, :] * 1.4, 0, 255).astype(np.uint8)
        canvas[0:coil_h, :] = coil_canvas
        alpha[0:coil_h, :] = 255

    # 2) 顶部框架 (固定 8px, 紧跟 coil)
    y_top_start = coil_h
    top_frame = door_rgba[0:TOP_FRAME, :, :3]
    canvas[y_top_start:y_top_start + TOP_FRAME, :] = top_frame
    alpha[y_top_start:y_top_start + TOP_FRAME, :] = 255

    # 3) 暗缝 (固定 6px, 不透明, 作为门体阴影)
    y_gap_start = y_top_start + TOP_FRAME
    dark_gap = door_rgba[TOP_FRAME:TOP_FRAME + DARK_GAP, :, :3]
    # 如果原图 gap 太浅, 加深一点
    dark_gap = (dark_gap * 0.65).astype(np.uint8)
    canvas[y_gap_start:y_gap_start + DARK_GAP, :] = dark_gap
    alpha[y_gap_start:y_gap_start + DARK_GAP, :] = 255

    # 4) 可见 slat (填充剩余空间)
    y_slat_start = y_gap_start + DARK_GAP
    if visible > 0:
        # slat 从 door y=14 (TOP_FRAME+DARK_GAP) 开始
        visible_slat = door_rgba[TOP_FRAME + DARK_GAP:TOP_FRAME + DARK_GAP + visible, :, :3]
        # 只填充到 canvas 底部
        fill_h = min(visible, CANVAS_H - y_slat_start)
        if fill_h > 0:
            canvas[y_slat_start:y_slat_start + fill_h, :] = visible_slat[:fill_h, :]
            alpha[y_slat_start:y_slat_start + fill_h, :] = 255

    # 合并
    rgba = np.zeros((CANVAS_H, CANVAS_W, 4), dtype=np.uint8)
    rgba[:, :, :3] = canvas
    rgba[:, :, 3] = alpha

    out_path = f'{frames_dir}/rollingdoor_{i:02d}.png'
    Image.fromarray(rgba, 'RGBA').save(out_path, optimize=True)
    print(f'  帧{i}: coil={coil_h}px, 框架=8px, 暗缝=6px, 可见slat={visible}px, 卷起={rolled}px')

# ===== Base 处理: 保留现有透明背景 + 门洞涂黑 =====
print(f'\n处理 base: 保留透明背景 + 门洞涂黑 ({DOOR_X0}-{DOOR_X1}, {DOOR_Y0}-{DOOR_Y1-1})')
img2 = Image.open('Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png').convert('RGBA')
rgba2 = np.array(img2)

# 只修改门洞区域: RGB=0, alpha=255
rgba2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 0] = 0
rgba2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 1] = 0
rgba2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 2] = 0
rgba2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 3] = 255

Image.fromarray(rgba2, 'RGBA').save('Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png', optimize=True)
print(f'Base 已处理: 保留现有 alpha, 门洞涂黑 72x75')

# ===== 预览 =====
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
    a = f[:, :, 3:4] / 255.0
    bg[28:, i * CANVAS_W:(i + 1) * CANVAS_W, :3] = (
        f[:, :, :3] * a + bg[28:, i * CANVAS_W:(i + 1) * CANVAS_W, :3] * (1 - a)
    ).astype(np.uint8)
    bg[28:, i * CANVAS_W:(i + 1) * CANVAS_W, 3] = 255

img_pil = Image.fromarray(bg, 'RGBA')
draw = ImageDraw.Draw(img_pil)
for i in range(N_FRAMES):
    draw.text((i * CANVAS_W + CANVAS_W // 2 - 5, 6), f'#{i}', fill=(255, 200, 100, 255))
img_pil.save('tools/_frames_final_strip_v5.png', optimize=True)
print(f'  tools/_frames_final_strip_v5.png')

# 在 base 上叠加预览
preview_base = rgba2.copy()
frame0 = frames[0]
# frame0 底中 pivot 对齐门洞底 (560, 699)
# frame0 左下角 = (560 - 36, 699 - 75 + 1) = (524, 625)
px, py = DOOR_X0, DOOR_Y0
a = frame0[:, :, 3:4] / 255.0
preview_base[py:py + CANVAS_H, px:px + CANVAS_W, :3] = (
    frame0[:, :, :3] * a + preview_base[py:py + CANVAS_H, px:px + CANVAS_W, :3] * (1 - a)
).astype(np.uint8)
preview_base[py:py + CANVAS_H, px:px + CANVAS_W, 3] = 255
Image.fromarray(preview_base, 'RGBA').save('tools/_preview_on_base_v5.png', optimize=True)
print(f'  tools/_preview_on_base_v5.png (frame0 叠在 base 门洞上)')
print('完成!')
