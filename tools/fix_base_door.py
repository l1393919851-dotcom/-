from PIL import Image
import numpy as np
import os

src = 'design/art/generated/2D_16_bit_pixel_art__1280x720__2026-07-22T03-57-07.png'
out_path = 'Assets/Art/Buildings/NeonTower/Sprites/bldg_neon_tower_base.png'

img = Image.open(src).convert('RGB')
arr = np.array(img)

# 门洞区域涂纯黑 (RGB=0)
# 门在 x=524-596, y=625-699 (72x75)
DOOR_X0, DOOR_X1 = 524, 596
DOOR_Y0, DOOR_Y1 = 625, 700
arr[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 0] = 0
arr[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 1] = 0
arr[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1, 2] = 0

os.makedirs(os.path.dirname(out_path), exist_ok=True)
Image.fromarray(arr, 'RGB').save(out_path, optimize=True)

print(f'Resaved {out_path}')
print(f'  door blackened: ({DOOR_X0},{DOOR_Y0})-({DOOR_X1},{DOOR_Y1})')

# 验证
img2 = Image.open(out_path)
print(f'  mode: {img2.mode}')
arr2 = np.array(img2)
print(f'  door avg RGB: {arr2[DOOR_Y0:DOOR_Y1, DOOR_X0:DOOR_X1].mean(axis=(0,1))}')
