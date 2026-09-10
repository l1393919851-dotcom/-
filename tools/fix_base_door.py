from PIL import Image
import numpy as np
import os

src = 'design/art/generated/2D_16_bit_pixel_art__1280x720__2026-07-22T03-57-07.png'
out_path = 'Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png'

img = Image.open(src).convert('RGB')
arr = np.array(img).astype(np.float32)

r, g, b = arr[:,:,0], arr[:,:,1], arr[:,:,2]
avg = (r + g + b) / 3.0
sat = np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b)

# 识别原图的浅灰/白色背景 (light + low saturation)
bg_mask = (avg > 200) & (sat < 20)

# 右下角水印小字区域
watermark_mask = np.zeros_like(bg_mask, dtype=bool)
watermark_mask[680:720, 1050:1280] = True
watermark_mask &= (avg > 180)

combined_mask = bg_mask | watermark_mask

# 门洞区域涂纯黑 (RGB=0)
# 门在 x=524-596, y=625-699 (72x75)
DOOR_X0, DOOR_X1 = 524, 596
DOOR_Y0, DOOR_Y1 = 625, 700
arr[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1] = [0, 0, 0]

# 转为 RGBA, 背景 (combined_mask) 的 alpha=0, 其他 alpha=255
out_rgb = arr.astype(np.uint8)
rgba = np.zeros((out_rgb.shape[0], out_rgb.shape[1], 4), dtype=np.uint8)
rgba[:, :, :3] = out_rgb
rgba[:, :, 3] = np.where(combined_mask, 0, 255)

os.makedirs(os.path.dirname(out_path), exist_ok=True)
Image.fromarray(rgba, 'RGBA').save(out_path, optimize=True)

print(f'Resaved {out_path}')
print(f'  background transparent pixels: {combined_mask.sum()}')
print(f'  door blackened (opaque): ({DOOR_X0},{DOOR_Y0})-({DOOR_X1},{DOOR_Y1})')

# 验证
img2 = Image.open(out_path)
arr2 = np.array(img2)
print(f'  mode: {img2.mode}')
print(f'  alpha=0 pixels: {(arr2[:,:,3] == 0).sum()}')
print(f'  alpha=255 pixels: {(arr2[:,:,3] == 255).sum()}')
print(f'  door alpha range: {arr2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 3].min()}-{arr2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 3].max()}')
