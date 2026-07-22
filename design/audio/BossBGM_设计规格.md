# BOSS 战 BGM 设计规格 — 僵尸博士 / 融合怪

> **项目**：赛博不夜城（Cyber Never Sleeps）
> **文档类型**：音频方向 · BGM 设计规格
> **设计师**：阮和鸣（Ruan Hemo）
> **版本**：v1.0
> **日期**：2025-07-21
> **状态**：待审批 → 待生成试听

---

## 目录

1. [BGM 定位](#1-bgm-定位)
2. [战斗结构映射（BGM-Fight Structure Map）](#2-战斗结构映射)
3. [音乐结构](#3-音乐结构)
4. [乐器与声场](#4-乐器与声场)
5. [AI 生成提示词（最重要）](#5-ai-生成提示词最重要)
6. [参考对标](#6-参考对标)
7. [与项目其他设计的一致性](#7-与项目其他设计的一致性)
8. [Unity 音频实现规格](#8-unity-音频实现规格)
9. [混音与总线说明](#9-混音与总线说明)
10. [风险与下一步](#10-风险与下一步)

---

## 1. BGM 定位

### 一句话定义

> **一条"2 分半钟内从被压迫到反杀"的工业赛博电子战歌——玩家不是在逃跑，而是在废墟中与疯狂博士正面对决。**

### 情感目标

| 维度 | 目标 | 说明 |
|------|------|------|
| 第一感觉 | **压迫感** | BOSS 出场即有碾压级别的低频压迫，让玩家感到"这不是普通战斗" |
| 战斗中期 | **壮烈感** | 旋律在压抑中上升——玩家不是在虐菜，而是在末日中挣扎反击 |
| 狂暴阶段 | **失控感** | BPM 提升、节奏碎裂、声场失稳——映射博士进入二阶段的疯狂 |
| 胜利时刻 | **释放感** | 战斗结束的尾奏——不是欢庆，而是战后废墟中的喘息与肃穆 |
| 失败时刻 | **坠落感** | 渐弱 + 低频嗡鸣 + 故障失真——玩家倒下后世界逐渐模糊 |

**绝对不要**：欢快的、英雄主义的、迪士尼式的胜利旋律。这里是赛博末日，不是超级英雄电影。

---

## 2. 战斗结构映射

### 战斗流程（参考游戏概念文档 §2.1.4 / §4.5）

僵尸博士战分为 **2 个主要阶段**，融合怪战为 **1 个阶段 + 1 个狂暴**：

#### 融合怪（The Amalgam）

| 战斗阶段 | 时长（预估） | BGM 段落 | 音乐手法 |
|----------|------------|---------|---------|
| 进入能量塔区域 | ~8s | Intro（前奏） | 环境音 + 低频嗡鸣渐入 |
| 融合怪出场 | ~5s | Hit（冲击） | 失真贝斯 + 工业噪音 + 画面黑屏闪电音 |
| 第一阶段（本体战） | ~60s | Verse A（主段） | 稳定的四拍节奏 + 旋律 loop |
| 狂暴阶段（40% 血量） | ~40s | Verse B（变奏） | BPM +15，增加不和谐音程，节奏断裂 |
| 击败 | ~5s | Outro-A（胜利） | 能量核心崩溃声 + 残响衰减 |

#### 僵尸博士（Dr. Plague） — 最终战

| 战斗阶段 | 时长（预估） | BGM 段落 | 音乐手法 |
|----------|------------|---------|---------|
| 进入博士实验室 | ~10s | Intro（前奏） | 紫色光晕 + 低沉弦乐/合成器 pad |
| 博士出场 + 召唤融合怪守卫 | ~8s | Hit（冲击） | 博士主题旋律首次完整呈现 |
| 守卫战（融合怪 × 2） | ~50s | Verse A | 节奏驱动，主题低音化变奏 |
| 博士亲自参战（第一阶段） | ~40s | Verse A' | 主题旋律上升八度，加入打击乐 |
| 博士狂暴（第二阶段，50% 血量） | ~40s | Verse C（炸裂） | BPM 160+，鼓双倍速，声场碎裂 |
| 最终一击 QTE / 终结 | ~6s | Climax | 全频段白热化 + 突然骤停 |
| 击败 | ~8s | Outro-B（坠落） | 博士实验服崩塌声→ 安静→ 唯一的心跳声 |

**总时长目标**：2:30 - 3:00（包含完整的 Intro → Verse A → Verse B/A' → Verse C → Outro 结构）

---

## 3. 音乐结构

### 完整结构总览（建议 2:45）

```
[0:00] Intro (前奏) — 8-10 秒
│  · 低频铺底（sub-bass drone）：C2 持续音
│  · 故障电子脉冲（glitch pulse）：1/4 音符节奏
│  · 环境层：远处警报声、液体流动声（采样自实验室环境）
│  · 声场：极宽（L/R 各一个故障脉冲对位），低频集中在中央
│  · 动态：pp → mp
│
[0:08] Hit (冲击) — 4-5 秒
│  · 全频段 silence（留白 0.5 秒）→ 爆发
│  · 失真贝斯（distorted bass）强力齐奏 + 工业金属撞击声
│  · 音高：下降的滑音 (C2 → G1) 制造坠落感
│  · 混响：大 Hall reverb 衰减 2s
│
[0:13] Verse A (主段) — 50-60 秒
│  · BPM：145
│  · 节奏：4/4 拍，kick on 1 & 3, snare/clap on 2 & 4
│  · 贝斯：锯齿波（saw wave）bassline，C 小调，重复 riff
│  · 主旋律：合成器 lead（square wave + 轻微失真），博士主题旋律
│  · 和声进行：Cm → B♭m → A♭m → Gdim（持续下行，制造压迫感）
│  · 打击乐层：工业打击乐 + 电子军鼓 + hi-hat 16分音符
│  · 声场：居中，宽度 70%
│  · 动态：mf
│
[1:03] Build-up (攒势) — 8-10 秒
│  · hi-hat 频率加倍（32分音符）
│  · riser 上升音效（white noise sweep + 音高上升）
│  · 滤波器截止频率逐渐打开（low-pass → 全频段）
│  · 每 2 个小节加入一个 "能量鼓点"（taiko + 混响）
│
[1:13] Verse B (变奏 / 狂暴) — 40-45 秒
│  · BPM：160（+15）
│  · 节奏：鼓变为 half-time feel（kick on 1, snare on 3），hi-hat 32分音符保持速度感
│  · 贝斯：低频失真加剧，加入 sub-bass 层（C1 持续音）
│  · 主旋律：变形——原旋律被切分成故障碎片（glitch chops），随机排列
│  · 新元素：工业警示音（alarm-like synth stab），每 4 小节一次
│  · 声场：异常宽——极左/极右各一个反相位的故障旋律碎片
│  · 混响：从小厅切换到巨大洞穴混响（模拟博士实验室空间失稳）
│  · 失真：master bus 轻微过载（模拟音响系统不堪重负）
│  · 动态：ff
│
[1:53] Climax (高潮) — 10-15 秒
│  · 所有元素全频段爆发
│  · kick 和 bass 合二为一（sidechain compression 达到极限）
│  · 旋律：叠置两个相差半音的主题（C 调 + C# 调同时演奏 → 不和谐张力）
│  · riser 持续上升 → 突然骤停（tape stop 效果）
│  · silence 1 秒（玩家 QTE 或最终一击的时刻）
│  · 然后：一个巨大而沉重的 "定音鼓 + 工业砸击" 作为终结
│
[2:08] Outro (尾声) — 8-10 秒
│  · 胜利：快速 fadeout + 残余的低频嗡鸣 → 安静 → 1-2 秒的心跳声（玩家的心跳）
│  · 失败：渐弱 + 低频嗡鸣 + 故障失真（glitch corruption）→ 持续 3 秒低频 drone → fade to black
│
[2:18] 结束
```

### 和声框架（C 小调为主，贯穿全曲）

```
Verse A:
| Cm   | B♭m  | A♭m  | Gdim  |  × 4

Verse B:
| Cm/F | Fm    | Cm/G  | G7sus4|  × 2
| Cm   | B♭m  | A♭m  | G/F   |  × 2

Climax:
| Cm   | ——   | C#m  | ——   |  ——同时演奏制造不和谐
```

**为什么要用持续下行和声？**
- 每个和弦都在"下降"，映射玩家被博士一步步逼入绝境
- 结尾的 Gdim（减三和弦）创造"悬而未决"的紧张感
- 狂暴阶段插入 G7sus4，增加不稳定的悬挂感

---

## 4. 乐器与声场

### 4.1 合成器与电子音色清单

| 声音层 | 推荐音色 | 合成器/音源建议 | 说明 |
|--------|---------|---------------|------|
| **Bass (主体)** | 锯齿波失真贝斯（Saw bass + distortion） | Serum / Vital / Massive | 粗粝、有攻击性，低频饱满 |
| **Sub Bass (底层)** | 正弦波 sub（Sine sub） | 任何合成器 | 仅低于 100Hz，物理感 |
| **Lead 旋律** | Square wave + 轻微失真 + 滑音 | Serum / Arturia Mini V | 像素游戏的"8-bit 进化版"——保留方波的锐利但更厚实 |
| **Pad** | 滤波后的超级锯齿（Filtered supersaw） | Sylenth1 / Serum | 铺底氛围，带缓慢的 filter mod |
| **Arp** | 16分音符递进琶音 | 内置 arp 或手动编程 | 增加紧迫感，音高在 C4-C6 区间 |
| **Stab** | 工业警示合成器 stab | Hardware: MS-20 仿真 | 带尖锐的滤波器爆破声 |
| **Glitch 碎片** | 切碎再拼接的旋律采样 | Effectrix / Gross Beat | 狂暴阶段的旋律碎片化效果 |
| **噪声 riser** | White noise + 带通滤波 + 音高上升 | 任何合成器 | 用于段落过渡 |
| **打击乐噪声** | 工业撞击 + 金属碰撞 + 破碎玻璃 | 采样库 | 不和谐音效点缀 |

### 4.2 鼓组与打击乐

| 元素 | 声音描述 | 参考方向 |
|------|---------|---------|
| **Kick** | 厚重、压缩感强、带 808 风格的 sub 尾音 | Justice / Gesaffelstein 风格的 kick |
| **Snare** | 电子军鼓 + 房间 noises，带长 decay | Porter Robinson 早期作品的 snare 处理 |
| **Clap** | Industrial clap，带金属混响 | The Prodigy 风格的 clap |
| **Hi-hat** | 清脆的电子 hi-hat，16/32 分音符 | 典型电子舞曲处理 |
| **Ride / Crash** | 工业感，带失真和 gate reverb | 每段落结束的标记音 |
| **Taiko / 大鼓** | 低频冲击 + 大厅混响 | 用于 build-up 的能量鼓点 |
| **工业打击** | 铁管碰撞、金属板震动、液压声 | 采样自真实工业声源 |

### 4.3 BPM 与节奏

| 段落 | BPM | 节奏特征 |
|------|-----|---------|
| Intro | 自由节奏（无律动） | 氛围主导，没有明显的 kick |
| Verse A | **145** | 四拍地板节奏，稳定推进 |
| Build-up | 145 → 160 | 通过 riser + hi-hat 加速感 |
| Verse B | **160** | 鼓半速但 hi-hat 加倍 = 双倍压迫 |
| Climax | 160 | 全频爆发，节奏几乎碎裂 |

### 4.4 声场布局（Stereo Field）

```
         LEFT ←————————————— CENTER —————————————→ RIGHT
               │                    │
  Verse A:     │    lead（中偏左）   kick+bass（C）   lead（中偏右）
               │    arp（左 60%）    snare（C）        arp（右 60%）
               │    工业噪音（左80%） pad（C）         hi-hat（右 50%）
               │                                          
  Verse B:     │    glitch L（90%）  kick+bass（C）    glitch R（90%）
               │    alarm stab L     snare（C）        alarm stab R
               │    破碎旋律（70%）  sub bass（C）      破碎旋律（70%）
               │    pad L（50%）     massive reverb     pad R（50%）
```

**设计意图**：
- 第一阶段声场较集中，玩家感到"有空间但被包围"
- 第二阶段声场拉伸到极致，营造"世界在解体"的失重感
- 低频永远居中（kick + sub bass mono 兼容）

---

## 5. AI 生成提示词（最重要）

### 5.1 提示词使用说明

| 项目 | 建议值 |
|------|--------|
| 推荐工具 | **Suno v4**（旋律理解最好）/ **Udio**（音色质感更好） |
| 推荐模型 | Suno v4 或 Udio 1.5 |
| 生成长度 | 先试 2 分钟版，满意后扩展至 3 分钟 |
| 关键设置 | Instrumental（纯器乐，不唱歌词） |
| 生成策略 | 先用完整提示词生成 → 不满意则分段落生成后拼接 |

> ⚠️ **重要**：AI 音乐工具对"游戏配乐"风格的理解有限，建议首批生成 5-10 个变体后筛选最佳段落，再用 ChatGPT/Claude 分析筛选出最优组合进行二次生成。

---

### 5.2 主提示词（Full Track）— 推荐先试这个

#### 英文（Suno v4 / Udio 首选）

```
[Genre Tags]
dark electro, cyberpunk boss theme, industrial electronic, 
dark synthwave, orchestral electronic hybrid, 
dark cinematic battle music, aggressive electronic

[Instruments & Sound Design]
heavy distorted saw wave bass, deep sub bass drone, 
aggressive square wave synth lead with portamento, 
pulsating filtered supersaw synth pad, 
16th note arpeggiated synthesizer, 
industrial alarm synth stabs, glitchy chopped melody fragments, 
white noise risers, tape stop effects
industrial metal percussion, heavy compressed kick drum, 
electronic snare with room reverb, crisp hi-hats at 32nd notes, 
taiko drum hits with huge reverb, metallic crash samples, 
industrial clang and hydraulic press sound effects

[Musical Structure & Arrangement]
0:00-0:08 intro: dark sub bass drone, flickering electronic pulses, distant siren ambience
0:08-0:12 hit: full stop silence then distorted bass explosion with descending pitch slide
0:12-1:02 verse A: steady 145 BPM 4/4 beat, driving bassline in C minor
  chord progression: Cm → B♭m → A♭m → Gdim descending
  lead melody: aggressive square wave, sinister theme
1:02-1:12 build-up: hi-hat doubles, filter sweep opening, tension rising
1:12-1:52 verse B: 160 BPM half-time drums, 32nd note hi-hats
  bass layer doubled with sub bass drone
  lead melody fragmented into glitch chops
  dissonant chord stab every 4 bars
  extreme stereo widening on glitch elements
1:52-2:07 climax: full frequency explosion, stereo field chaos
  two lead synths playing C and C# simultaneously (dissonant)
  sudden tape stop into 1 second silence
  single massive industrial impact hit
2:07-2:18 outro: fadeout into low drone, then heartbeat sound effect

[Mood & Atmosphere]
oppressive, desperate, intense, aggressive,
post-apocalyptic cyberpunk, laboratory gone mad,
the feeling of a final boss pushing you to your limits,
NOT heroic, NOT triumphant — this is survival against a mad scientist,
dark cinematic energy, emotional weight without being melodramatic

[Production Style]
sidechain compression on bass and pad to kick drum,
wide stereo field in B section, narrow and focused in A section,
slight master bus saturation suggesting overload,
large hall reverb on industrial percussion,
clean digital distortion on synth leads,
sub frequencies kept mono, above 120Hz can be stereo

[Genre-Tags-end]
dark electro, cyberpunk, industrial electronic, dark synthwave, cinematic battle

[Keywords]
instrumental, no vocals, no singing, no lyrics,
game boss theme, video game ost style,
cyberpunk dark electronic battle music
```

#### 中文提示词（备用，适合国内工具）

```
[风格标签]
暗黑电子, 赛博朋克BOSS战主题, 工业电子,
暗黑合成波, 电子管弦混合, 电影感暗黑战斗音乐, 激进电子

[乐器和音色设计]
重型失真锯齿波贝斯, 深沉次低音嗡鸣,
激进的方波合成器主旋律带滑音,
脉冲滤波超锯齿合成铺底,
十六分音符琶音合成器,
工业警报合奏音色, 故障化切碎的旋律碎片,
白噪上升音效, 磁带骤停效果
工业金属打击乐, 重型压缩底鼓,
带房间混响的电子军鼓, 清脆的三十二分音符踩镲,
带巨大混响的太鼓重击, 金属碰撞采样,
工业铁管碰撞和液压声效果

[音乐结构和编曲]
0:00-0:08 前奏: 深沉低音嗡鸣, 闪烁电子脉冲, 远处警报氛围
0:08-0:12 冲击: 全段静音后失真贝斯爆裂加下降滑音
0:12-1:02 主段落A: 稳定145 BPM 4/4拍, C小调驱动贝斯线
  和声进行: Cm → B♭m → A♭m → Gdim 持续下行
  主旋律: 激进方波合成器, 邪恶感
1:02-1:12 聚势: 踩镲频率加倍, 滤波器打开, 紧张感上升
1:12-1:52 主段落B: 160 BPM 半速鼓, 三十二分音符踩镲
  贝斯加倍加次低音嗡鸣
  主旋律切碎成故障碎片
  每4小节不和谐和弦冲击
  极宽立体声场
1:52-2:07 高潮: 全频段爆发, 立体声场混乱
  两个主旋律合成器同时演奏C和C#(不和谐音)
  突然磁带骤停进1秒静音
  单次巨大工业重击
2:07-2:18 尾声: 渐弱到低频嗡鸣, 然后心跳声

[情感和氛围]
压抑, 绝望, 激烈, 激进,
后末日赛博朋克, 疯狂的实验室,
最终BOSS把你逼到极限的感觉,
不是英雄主义, 不是胜利感——这是在疯狂博士面前求生存,
暗黑电影能量, 有情感重量但不煽情

[制作风格]
侧链压缩贝斯和铺底跟随底鼓,
B段立体声场宽, A段集中狭窄,
母带轻微饱和模拟过载,
工业打击乐用大厅混响,
合成器主旋律用干净的数码失真,
次低频保持单声道, 120Hz以上可以立体声

[关键词]
纯器乐, 无人声, 无歌词,
游戏BOSS主题, 游戏原声风格,
赛博朋克暗黑电子战斗音乐
```

---

### 5.3 分段落生成提示词

如果整曲生成质量不佳，建议分段生成后手动拼接：

#### Part 1: Intro + Hit 冲击入场（0:00-0:12）

```
dark cyberpunk boss intro, 10 seconds
deep sub bass drone in C2, flickering electronic pulse at 1/4 note rhythm,
distant alarm ambience, sudden silence then explosive distorted bass hit 
with descending pitch slide C2 to G1,
industrial metal impact sound, huge hall reverb decay 2 seconds,
atmosphere: oppressive laboratory, something terrible emerging
instrumental, no vocals
```

#### Part 2: Verse A 战斗主段（0:12-1:02）

```
aggressive cyberpunk battle theme, 50 seconds
145 BPM 4/4 beat, driving distorted saw wave bass in C minor,
chord progression Cm B♭m A♭m Gdim (descending),
aggressive square wave synth lead melody with portamento,
16th note arpeggio, pulsating filtered supersaw pad,
heavy compressed kick on 1&3, electronic snare with room reverb,
industrial percussion layer,
atmosphere: intense desperate survival against a mad scientist boss,
instrumental, video game boss theme style, no vocals
```

#### Part 3: Verse B 狂暴变奏（1:12-1:52）

```
dark cyberpunk boss rage phase, 40 seconds
160 BPM half-time drums, double speed hi-hats 32nd notes,
distorted bass doubled with deep sub bass drone C1,
original lead melody chopped into glitch fragments, randomly reordered,
dissonant alarm synth stab every 4 bars, extreme stereo widening,
huge cavern reverb, slight master bus distortion overload,
atmosphere: laboratory reality breaking apart, boss going insane,
instrumental, no vocals, video game ost
```

#### Part 4: Climax + Outro 高潮与终结（1:52-2:18）

```
cyberpunk boss climax and ending, 25 seconds
full frequency explosion, two lead synths playing C and C# 
simultaneously for dissonance,
sudden tape stop into 1 second of complete silence,
one massive final impact hit (taiko + industrial crash),
fast fadeout into residual low frequency drone,
then 2 beats of heartbeat sound (player's heart),
atmosphere: aftermath of desperate battle, silence in the ruins,
instrumental, no vocals
```

---

### 5.4 Suno v4 特殊优化提示（单独使用）

```
[MODE: INSTRUMENTAL]
[STYLE: dark electro industrial cyberpunk cinematic boss battle]
[KEY: C minor]
[BPM: 145]
[INSTRUMENTS: distorted saw bass, square wave lead synth, 
  industrial drums, sub bass drone, arpeggiator]
[MOOD: oppressive, intense, desperate, aggressive, dark cinematic]
[STRUCTURE: intro (8s) → hit (4s) → verse (50s) → build-up (10s) → 
  rage verse (40s) → climax (15s) → outro (10s)]
[RULES: no vocals, no singing, no guitar solo, no orchestral strings]
```

---

## 6. 参考对标

### 参考曲目清单

| # | 曲目 | 来源 | 借鉴方向 | 具体参考点 |
|---|------|------|---------|-----------|
| 1 | **"The Rebel Path (Cynosure Version)"** | Cyberpunk 2077: Phantom Liberty OST | **整体基调**——赛博朋克BOSS战的工业电子表达 | 贝斯线的压迫感、合成器lead的尖锐与情感、鼓组的电子工业质感。最接近"赛博朋克末日BOSS战"的听觉想象 |
| 2 | **"Roller Mobster"** | Carpenter Brut / Hotline Miami 2 OST | **Darksynth 能量与节奏**——高BPM下的冲击力 | 重型电子贝斯的处理方式、粗粝的合成音色、稳定的4/4节奏如何在高速下保持压迫感。Carpenter Brut 的暗黑合成波（darksynth）是赛博朋克游戏的黄金标准 |
| 3 | **"BFG Division"** | DOOM (2016) OST — Mick Gordon | **工业金属与电子的融合方式**——如何在游戏配乐中制造"重量感" | kick和bass的低频设计、失真程度的把控、节奏性的工业噪声运用。虽然DOOM更重金属，但它的低频处理 philosophy 可以直接移植 |
| 4 | **"Turbo Killer (Instrumental)"** | Carpenter Brut | **从压抑到爆发的结构**——完美的build-up和drop结构 | 段落过渡的手法、频率能量的积累与释放、音色选择的一致性。可以作为"一条2分半钟怎么安排情绪曲线"的结构模板 |

**不建议参考的**：
- 交响乐团为主的史诗BOSS战（如 Dark Souls）——不是本游戏的听觉语言
- 纯8-bit/chip tune 风格——虽然游戏是像素风，但BOSS战需要更厚重的低频支撑
- 重金属吉他为主的曲目——与赛博朋克的合成器美学冲突

---

## 7. 与项目其他设计的一致性

### 7.1 与赛博朋克视觉语言的映射

| 视觉元素（美术圣经） | 音乐映射 |
|---------------------|---------|
| 赛博紫 `#B026FF` — 能量场、核心UI强调色 | 合成器 lead 的音色——尖锐、带轻微失真、在中高频有存在感 |
| 数据蓝 `#00F0FF` — 玩家势力色、全息投影 | 琶音器的音色——清晰、冷冽、有数字感（digitized） |
| 血光粉 `#FF003C` — 危险/警报 | 工业警报 stab——刺耳、脉冲感、在混音中"割裂"出来 |
| 暗夜黑 `#0A0E14` — 黑暗基底 | 低频 sub drone、silence 的运用——黑暗的"听觉等价物" |
| 混凝土灰 `#3A3F4A` — 废墟质感 | 工业打击乐的粗粝感——不精致、不抛光 |
| 毒雾绿 `#39FF14` — 能量核心 | 狂暴阶段的不和谐音程——危险的能量泄漏感 |
| 霓虹灯管故障闪烁 | glitch chop 效果——旋律被"切碎"再"闪烁" |
| 全息投影扫描线 | LFO 自动滤波——声音如扫描线般周期性开合 |
| 能量光柱 | frequency riser——从低频到高频的"上升光柱" |

### 7.2 与像素艺术的协调

| 像素游戏特性 | BGM 适配策略 |
|------------|-------------|
| 角色像素小（32×48） | 音乐不过度复杂——clean arrangement，避免多层密集旋律导致"听感像素糊" |
| 俯视视角 + 网格 | 节奏保持稳定（四拍地板），给玩家明确的"步调感"——节奏是听觉的网格线 |
| 12 FPS 动画 | BGM 律动不要与动画帧率打架——BPM 145 下的 1/4 音符 = 约 83ms，接近 12FPS 帧间隔，形成隐约的节奏一致性 |
| 屏幕信息密集（生存/建造/战斗） | BGM 在 Verse A 阶段保持可预测的 loop——玩家的大脑需要一部分"可预测"来补偿视觉的高信息量 |
| 像素硬边美学（禁止抗锯齿） | 合成器音色选择"硬边"音色（square wave, saw with low smoothing），避免圆润的 pad 音色 |

### 7.3 与叙事的一致性（参考游戏概念文档）

| 叙事设定 | BGM 体现 |
|---------|---------|
| 博士是前首席科学家，高等智力 | 主旋律有"邪恶的优雅"——不是野蛮的兽性，而是扭曲的智慧感 |
| 化合物/实验室设定 | 环境层加入液体声、电子脉冲、实验室呼吸声等非音乐元素 |
| 4个能量塔守卫 → 最终博士战 | 每条 BGM 共享一个"博士主题"旋律动机，能量塔战为低音区变奏，最终战为全音域完整版 |
| 无限复活的终局刷取 | BGM 在终局刷取时提供轻微变体（随机化的 glitch 元素），让重复战斗不疲劳 |
| "不是丧尸+赛博朋克的皮肤叠加" | 音乐不是"随便一段电子音乐"——博士主题有明确的旋律identity，贯穿所有战斗 |

---

## 8. Unity 音频实现规格

### 8.1 音频中间件建议

| 项 | 建议 |
|---|------|
| 中间件 | **Unity 原生 Audio Mixer + Timeline**（轻量方案），或 **FMOD** / **Wwise**（进阶方案） |
| 推荐路线 | 项目体量下 Unity 原生 + Timeline 足够处理 BGM 的段落切换 |
| 理由 | BOSS 战的段落切换是线性的（Intro → A → B → C → Outro），不需要 Wwise 的复杂交互音乐系统 |

### 8.2 BGM 分段命名规范

```
BGM_BOSS_DRPLAGUE_INTRO         → 前奏（intro）
BGM_BOSS_DRPLAGUE_HIT           → 冲击（hit）
BGM_BOSS_DRPLAGUE_VERSE_A       → 主段（verse A）
BGM_BOSS_DRPLAGUE_BUILDUP       → 聚势（build-up）
BGM_BOSS_DRPLAGUE_VERSE_B       → 变奏狂暴（verse B）
BGM_BOSS_DRPLAGUE_CLIMAX        → 高潮（climax）
BGM_BOSS_DRPLAGUE_OUTRO_VICTORY → 胜利尾声
BGM_BOSS_DRPLAGUE_OUTRO_DEFEAT  → 失败尾声
```

### 8.3 音频事件清单

| 事件名称 | 触发条件 | 优先级 | 变体数 | 说明 |
|---------|---------|--------|-------|------|
| `evt_bgm_boss_intro` | 进入BOSS区域（触发器） | High | 1 | BGM开始 |
| `evt_bgm_boss_verse_a` | 融合怪/博士出场动画结束 | High | 1 | 主战斗loop |
| `evt_bgm_boss_build_up` | BOSS血量 ≤ 60% | High | 1 | 过渡段落 |
| `evt_bgm_boss_verse_b` | BOSS血量 ≤ 40%（狂暴） | High | 1 | 变奏loop |
| `evt_bgm_boss_climax` | BOSS血量 ≤ 5%（可处决） | Critical | 1 | 高潮段 |
| `evt_bgm_boss_victory` | BOSS击败 | High | 1 | 胜利尾奏 |
| `evt_bgm_boss_defeat` | 玩家死亡 | High | 1 | 失败尾奏 |
| `evt_bgm_boss_transition_ab` | Verse A → Verse B 切换 | High | 1 | 平滑过渡 |

### 8.4 Timeline 实现建议（Unity 原生方案）

```
BOSS区域进入
    │
    ▼
[Timeline: BOSS_BGM_Sequence]
    │
    ├── Intro (8s) ───────────────→ Hit (4s)
    │                                    │
    │                                    ▼
    │                             Verse A (50s, loop)
    │                                    │
    │                              [Check: HP ≤ 60%?]
    │                                    │
    │                          Yes        No
    │                           │         │
    │                           ▼         └──→ continue Verse A
    │                     Build-up (8s)
    │                           │
    │                           ▼
    │                     Verse B (40s, loop)
    │                           │
    │                    [Check: HP ≤ 5%?]
    │                           │
    │                  Yes        No
    │                   │         │
    │                   ▼         └──→ continue Verse B
    │              Climax (12s)
    │                   │
    │            [BOSS defeated?]
    │                   │
    │         Yes        No
    │          │         │
    │          ▼         └──→ player death → Defeat outro
    │    Victory outro
    │
    ▼
   BGM End
```

### 8.5 性能预算

| 项目 | 预算 | 说明 |
|------|------|------|
| BGM 同发音数 | 1 | 一次只播放一条 BGM |
| BGM 文件大小 | ≤ 15 MB | 建议 256kbps MP3 或 OGG |
| 加载方式 | 预加载 | BOSS 区域进入前即加载至内存 |
| 过渡时间 | ≤ 100ms | 段落切换的 latency |

---

## 9. 混音与总线说明

### 9.1 混音目标

| 场景 | 混音优先级 | 说明 |
|------|-----------|------|
| 头盔/耳机 | ★★★ | PC 玩家多数用耳机，立体声声场设计关键 |
| 音箱 | ★★☆ | 保证 mono 兼容性（低频 mono、kick 居中） |
| 低质量扬声器 | ★★☆ | 中频清晰度 > 低频厚度 |

### 9.2 Mix 总线结构建议

```
Master Bus
├── BGM Bus
│   ├── Sub Bass (mono, < 100Hz)
│   ├── Bass (stereo, > 100Hz)
│   ├── Lead Synth (stereo, mid-side eq)
│   ├── Pad (stereo, wide)
│   ├── Arp (stereo)
│   ├── Glitch/FX (stereo, extreme width in Verse B)
│   ├── Kick (mono, center)
│   ├── Snare (mono, center)
│   ├── Hi-Hat (stereo)
│   └── Percussion/Industrial (stereo, hall reverb send)
│
├── SFX Bus (game sounds)
├── UI Bus (interface sounds)
└── VO Bus (if applicable — currently WON'T HAVE per MVP)
```

### 9.3 动态范围

| 段落 | 目标 LUFS | 说明 |
|------|-----------|------|
| Intro | -23 LUFS | 安静的氛围铺底 |
| Hit | -10 LUFS | 瞬间爆发（短时峰值） |
| Verse A | -16 LUFS | 主体战斗的能量级别 |
| Build-up | -14 → -10 LUFS | 逐渐压缩，听感越来越响 |
| Verse B | -11 LUFS | 狂暴阶段，持续高能量 |
| Climax | -8 LUFS（瞬时） | 白热化，但不超过 -3 dBTP |
| Outro | -20 → 渐弱至静音 | 释放与喘息 |

---

## 10. 风险与下一步

### 10.1 已知风险

| # | 风险 | 严重程度 | 缓解 |
|---|------|---------|------|
| R1 | AI 生成的 BGM 可能缺乏"段落切换的精确性"——段落之间的过渡需要人工微调 | 中 | 先整曲生成，再分段落优化+Daw拼接 |
| R2 | Suno/Udio 对纯器乐赛博朋克风格的理解可能不够精准，容易加入不想要的吉他或人声 | 高 | 使用精确的 Instrumental 模式 + 负面关键词过滤 |
| R3 | 多条 BOSS 战 BGM 共享同一主题但变体不同的策略可能需要额外生成工作 | 低 | 先生成博士战完整 BGM，融合怪战使用主题的低音简化版 |
| R4 | 160 BPM 的 Verse B 可能超出部分玩家的舒适节奏阈值 | 低 | 可通过 playtest 确认，必要时调降至 150-155 |

### 10.2 下一步建议

1. **用户审批**该 BGM 设计规格（音高方向、结构、乐器选择等）
2. 获批后，使用 Suno v4 按主提示词首批生成 **5-10 条候选**，阮和鸣筛选最优
3. 将最优候选的各段落拆分，在 DAW（Reaper / FL Studio / Ableton Live）中微调段落衔接
4. 在 Unity Timeline 中按 §8.4 的实现方案装配 BGM
5. 配合程基岩验证音频事件触发与 BOSS AI 血量的绑定逻辑
6. 联调后配合文策渊的 BOSS 战设计进行 playtest，确认 BGM 与战斗节奏的配合
7. 融合怪 BGM 作为变体复用此文档的框架（主题低音化、缩短结构、增加有机/血肉噪声元素）

### 10.3 待审批项

| # | 待审批项 | 建议 |
|---|---------|------|
| 1 | BPM 范围 145-160 是否合适？ | 155-160 的狂暴段在像素游戏中的可接受度需确认 |
| 2 | 是否同意"博士主题"在所有 BOSS 战中复用的策略？ | 同意则融合怪 BGM 将共享主题动机 |
| 3 | 先做完整曲还是分段生成后拼接？ | 建议先生成 2-3 条完整候选让用户试听，再精细调整 |
| 4 | 是否需要同时产出融合怪 BGM 规格？ | 可与博士 BGM 并行，融合怪为简化版变体 |

---

> **文档结束**
>
> 本规格为 v1.0 版本。用户审批后进入 AI 生成试听阶段。
>
> — 阮和鸣（Ruan Hemo），音频总监

## 附录：快速参考卡

### 一键提示词（最短版，适合快速丢进 Suno）

```
[Style: dark electro industrial cyberpunk boss battle]
[Key: C minor] [BPM: 145]
aggressive distorted saw bass, square wave synth lead,
pulsating pad, 16th note arp, heavy kick & snare,
industrial percussion, glitch effects.
Structure: intro(8s) → hit(4s) → verse(50s, Cm B♭m A♭m Gdim) 
→ build-up(10s) → rage verse(40s, 160BPM, glitch) 
→ climax(15s) → outro(10s, heartbeat)
Mood: oppressive, desperate, intense. NO vocals, NO guitar, NO orchestra.
```

### 色板 → 音色速查

| 视觉色 | 听觉等效 |
|--------|---------|
| `#B026FF` 赛博紫 | Square wave lead + mild distortion |
| `#00F0FF` 数据蓝 | Clean arp, 16th note pattern |
| `#FF003C` 血光粉 | Alarm synth stab, industrial crash |
| `#0A0E14` 暗夜黑 | Sub bass drone, silence |
| `#39FF14` 毒雾绿 | Dissonant interval (C + C# together) |
| 故障闪烁 | Glitch chop, tape stop effect |
