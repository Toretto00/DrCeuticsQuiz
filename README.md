# DrCeutics Quiz – Game đố vui cho standee cảm ứng

Game chạy trên **standee Windows** màn hình dọc (thiết kế 1080 × 1920, tự co giãn theo màn hình).

**Luật chơi:** mỗi lượt lấy ngẫu nhiên 3 câu từ bộ câu hỏi. Trả lời đúng từ 2/3 câu trở lên thì nhận quà.

Tài liệu này viết cho **người không biết lập trình**. Bạn chỉ cần làm đúng từng bước.

---

## Mục lục

1. [Các file bạn cần biết](#1-các-file-bạn-cần-biết)
2. [Thay đổi bộ câu hỏi](#2-thay-đổi-bộ-câu-hỏi)
3. [Thay đổi luật chơi (số câu, thời gian…)](#3-thay-đổi-luật-chơi)
4. [Cài đặt máy tính để build (làm 1 lần)](#4-cài-đặt-máy-tính-để-build-làm-1-lần)
5. [Chạy thử game trên máy tính](#5-chạy-thử-game-trên-máy-tính)
6. [Build ứng dụng cho standee](#6-build-ứng-dụng-cho-standee)
7. [Cài và vận hành trên standee](#7-cài-và-vận-hành-trên-standee)
8. [Gặp lỗi thì làm gì](#8-gặp-lỗi-thì-làm-gì)

---

## 1. Các file bạn cần biết

Bạn **chỉ cần đụng vào các file in đậm**. Những file còn lại là code, không cần mở.

```
DrCeuticsQuiz/
├── questions.json        ← BỘ CÂU HỎI + LUẬT CHƠI (sửa ở đây)
├── logo.png              ← LOGO DrCeutics hiện trên màn hình
├── Assets/
│   ├── olive.png         ← mascot Olive (ảnh mặc định)
│   ├── olive_happy.png   ← Olive vui (màn chờ, trả lời đúng)
│   ├── olive_think.png   ← Olive suy nghĩ (khi đang hỏi)
│   ├── olive_sad.png     ← Olive buồn (trả lời sai, thua)
│   ├── olive_wink.png    ← Olive nháy mắt (thắng)
│   └── Fonts/            ← font chữ Montserrat (không cần sửa)
├── publish-windows.sh    ← file dùng để build ra .exe (mục 6)
└── README.md             ← tài liệu bạn đang đọc
```

**Thay logo hoặc mascot:** chuẩn bị ảnh PNG nền trong suốt, đặt **đúng tên** như trên và chép đè vào đúng chỗ. Sau đó build lại (mục 6).

---

## 2. Thay đổi bộ câu hỏi

Toàn bộ câu hỏi nằm trong file **`questions.json`**. Bạn có thể sửa ở **một trong hai nơi**:

| Sửa ở đâu                                                  | Khi nào dùng                                           | Có phải build lại không?              |
| ---------------------------------------------------------- | ------------------------------------------------------ | ------------------------------------- |
| `questions.json` trong thư mục project                     | Muốn lưu bộ câu hỏi lâu dài, các lần build sau cũng có | **Có** (mục 6)                        |
| `questions.json` nằm cạnh `DrCeuticsQuiz.exe` trên standee | Sửa nhanh tại sự kiện                                  | **Không**, chỉ cần tắt game và mở lại |

> Sửa trực tiếp trên standee thì nhớ chép file đó về project. Nếu không, lần build sau sẽ ghi đè mất phần bạn đã sửa.

### 2.1. Mở file để sửa

- **Trên Mac:** mở bằng **VS Code** (khuyên dùng, vì VS Code báo lỗi nếu bạn gõ sai) hoặc TextEdit.
- **Trên Windows:** nhấp chuột phải vào file → **Open with** → **Notepad**.

### 2.2. Hiểu cấu trúc một câu hỏi

Mỗi câu hỏi là một khối nằm giữa cặp ngoặc nhọn `{ }`:

```json
{
  "question": "DrCeutics là thương hiệu dược mỹ phẩm của quốc gia nào?",
  "options": ["Việt Nam", "Hàn Quốc", "Pháp", "Nhật Bản"],
  "answer": 0,
  "explain": "DrCeutics là thương hiệu Việt, phát triển công thức phù hợp với làn da người Việt."
}
```

| Dòng         | Ý nghĩa                             | Lưu ý                                                                        |
| ------------ | ----------------------------------- | ---------------------------------------------------------------------------- |
| `"question"` | Nội dung câu hỏi                    | Nên dưới 100 chữ để vừa khung                                                |
| `"options"`  | Các đáp án                          | Nên có **4 đáp án**. Mỗi đáp án nằm trong ngoặc kép, ngăn nhau bằng dấu phẩy |
| `"answer"`   | **Vị trí** đáp án đúng              | **Đếm từ 0**: đáp án thứ 1 là `0`, thứ 2 là `1`, thứ 3 là `2`, thứ 4 là `3`  |
| `"explain"`  | Lời giải thích hiện ra sau khi chọn | Ngắn gọn, 1–2 câu                                                            |

**Ví dụ về `"answer"`:**

```
"options": ["Vitamin C", "Vitamin B3", "Vitamin A", "Vitamin E"]
                0             1            2            3
```

Đáp án đúng là "Vitamin B3" thì ghi `"answer": 1`.

> Bạn không cần lo người chơi đoán được vị trí đáp án. Khi hiển thị, game **tự xáo trộn** thứ tự các đáp án, nên đáp án đúng luôn để ở vị trí đầu (`"answer": 0`) cũng được và dễ nhớ nhất.

### 2.3. Sửa một câu hỏi có sẵn

1. Tìm câu cần sửa (trong VS Code nhấn `Cmd + F` để tìm chữ).
2. Chỉ sửa **phần chữ nằm giữa hai dấu ngoặc kép** `"..."`.
3. Lưu file (`Cmd + S` trên Mac, `Ctrl + S` trên Windows).

### 2.4. Thêm một câu hỏi mới

1. Kéo xuống **câu hỏi cuối cùng** trong file.
2. Ngay sau dấu `}` của câu cuối, gõ thêm **một dấu phẩy** `,`.
3. Dán khối mẫu dưới đây vào sau dấu phẩy đó, rồi sửa nội dung:
   ```json
   {
     "question": "Nội dung câu hỏi mới?",
     "options": ["Đáp án đúng", "Đáp án sai 1", "Đáp án sai 2", "Đáp án sai 3"],
     "answer": 0,
     "explain": "Giải thích ngắn."
   }
   ```
4. Lưu file.

Sau khi thêm, cuối file sẽ trông như thế này:

```json
    {
      "question": "Câu hỏi cũ cuối cùng?",
      ...
      "explain": "..."
    },          ← dấu phẩy vừa thêm
    {
      "question": "Nội dung câu hỏi mới?",
      ...
      "explain": "Giải thích ngắn."
    }           ← câu CUỐI CÙNG thì KHÔNG có dấu phẩy
  ]
}
```

### 2.5. Xoá một câu hỏi

1. Bôi đen cả khối từ dấu `{` đến dấu `}` của câu đó, **kèm dấu phẩy** ngay sau nó, rồi xoá.
2. Nếu bạn xoá **câu cuối cùng**, hãy xoá dấu phẩy ở cuối câu **liền trước** nó.

### 2.6. Năm lỗi hay gặp nhất (game sẽ không đọc được câu hỏi)

| ❌ Sai                                                        | ✅ Đúng                                          |
| ------------------------------------------------------------- | ------------------------------------------------ |
| Quên dấu phẩy giữa 2 câu hỏi: `} {`                           | `}, {`                                           |
| Thừa dấu phẩy ở câu cuối: `}, ]`                              | `} ]`                                            |
| Dùng ngoặc kép cong của Word: `“Việt Nam”`                    | Ngoặc kép thẳng: `"Việt Nam"`                    |
| Dùng dấu ngoặc kép bên trong câu: `"Sản phẩm "Olive" là gì?"` | Đổi thành ngoặc đơn: `"Sản phẩm 'Olive' là gì?"` |
| `"answer"` lớn hơn số đáp án, ví dụ có 4 đáp án mà ghi `4`    | Chỉ được ghi `0`, `1`, `2` hoặc `3`              |

**Mẹo kiểm tra:** mở file bằng VS Code. Nếu có chỗ gạch chân màu đỏ hoặc tab **Problems** báo lỗi thì file đang sai, rê chuột vào chỗ đó để xem lỗi gì. Nếu game không đọc được file, màn hình sẽ hiện câu hỏi _"Không đọc được questions.json…"_. Khi đó mở lại file và soát theo bảng trên.

> **Nên có ít nhất 6 câu hỏi** để mỗi lượt chơi khác nhau. Hiện có sẵn 10 câu.

---

## 3. Thay đổi luật chơi

Ở **đầu** file `questions.json` có khối `"config"`:

```json
"config": {
  "questionsPerGame": 3,
  "passScore": 2,
  "timePerQuestion": 20,
  "autoNextMs": 3500,
  "resultTimeout": 45,
  "idleTimeout": 60
}
```

| Tên                | Ý nghĩa                                                                           | Mặc định |
| ------------------ | --------------------------------------------------------------------------------- | -------- |
| `questionsPerGame` | Số câu hỏi mỗi lượt chơi                                                          | 3        |
| `passScore`        | Số câu đúng tối thiểu để nhận quà                                                 | 2        |
| `timePerQuestion`  | Số giây cho mỗi câu (hết giờ tính là sai)                                         | 20       |
| `autoNextMs`       | Thời gian chờ trước khi tự sang câu tiếp, tính bằng **mili giây** (1000 = 1 giây) | 3500     |
| `resultTimeout`    | Số giây màn kết quả tự quay về trang chủ                                          | 45       |
| `idleTimeout`      | Không ai chạm màn hình bao nhiêu giây thì game tự về trang chủ                    | 60       |

Chỉ sửa **con số**, giữ nguyên tên và dấu phẩy. Dòng cuối trong khối (`idleTimeout`) không có dấu phẩy.

> Nếu đổi `passScore`, nhớ sửa luôn câu "Trả lời đúng **2/3 câu hỏi**…" ở màn chờ. Câu này nằm trong file `MainWindow.axaml` (tìm chữ `2/3 câu hỏi`).

---

## 4. Cài đặt máy tính để build (làm 1 lần)

"Build" là bước biến code thành file `DrCeuticsQuiz.exe` để chạy trên standee. Bạn build được trên **Mac** hoặc **Windows**.

### 4.1. Trên Mac (M1/M2/M3)

1. **Cài .NET 8 SDK:** vào <https://dotnet.microsoft.com/download/dotnet/8.0>, ở cột **macOS** bấm **Arm64**, tải file `.pkg` và cài như phần mềm bình thường.
2. **Cài VS Code:** vào <https://code.visualstudio.com>, chọn bản **Apple Silicon**.
3. **Mở project:** mở VS Code → **File → Open Folder…** → chọn thư mục `DrCeuticsQuiz`.
4. Góc dưới bên phải VS Code hiện gợi ý cài extension thì bấm **Install** (gồm _C# Dev Kit_ và _Avalonia for VS Code_).
5. **Kiểm tra:** trong VS Code bấm menu **Terminal → New Terminal**, gõ lệnh sau rồi nhấn Enter:
   ```bash
   dotnet --version
   ```
   Hiện ra số bắt đầu bằng `8.` (hoặc lớn hơn) là cài thành công.

### 4.2. Trên Windows

1. **Cài .NET 8 SDK:** vào <https://dotnet.microsoft.com/download/dotnet/8.0>, ở cột **Windows** bấm **x64**, tải về và cài.
2. **Cài VS Code** (không bắt buộc, chỉ cần nếu muốn sửa code): <https://code.visualstudio.com>.
3. **Kiểm tra:** mở thư mục project, bấm vào thanh địa chỉ của cửa sổ thư mục, gõ `cmd` rồi Enter. Trong cửa sổ đen hiện ra, gõ:
   ```
   dotnet --version
   ```
   Hiện số `8.x.x` là được.

---

## 5. Chạy thử game trên máy tính

Nên chạy thử mỗi khi sửa câu hỏi, trước khi build cho standee.

1. Mở project bằng VS Code.
2. Mở Terminal (**Terminal → New Terminal**).
3. Gõ lệnh và nhấn Enter:
   ```bash
   dotnet run -- --window
   ```
   Lần đầu máy sẽ tải thư viện, mất khoảng 1–2 phút. Sau đó game mở trong một cửa sổ nhỏ. Bạn dùng chuột để chơi thử.
4. Muốn chạy toàn màn hình giống trên standee, gõ:
   ```bash
   dotnet run
   ```
5. **Thoát game:** nhấn tổ hợp **Control + Shift + Q** (trên Mac là phím **Control**, không phải Command).

---

## 6. Build ứng dụng cho standee

### Cách A: Build trên Mac

1. Mở project bằng VS Code, mở Terminal.
2. **Chỉ lần đầu tiên**, gõ lệnh sau để cho phép chạy file build:
   ```bash
   chmod +x publish-windows.sh
   ```
3. Gõ lệnh build:
   ```bash
   ./publish-windows.sh
   ```
4. Đợi khoảng 1–3 phút đến khi hiện chữ **"Xong!"**. Thư mục kết quả sẽ tự mở ra.

### Cách B: Build trên Windows

1. Mở thư mục project, bấm vào thanh địa chỉ, gõ `cmd` rồi Enter.
2. Dán lệnh sau vào cửa sổ đen và nhấn Enter:
   ```
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64
   ```
3. Đợi đến khi chạy xong, không còn chữ đỏ.

### Kết quả build

Cả hai cách đều tạo ra thư mục **`publish/win-x64/`** gồm:

| File                | Mô tả                                                                       |
| ------------------- | --------------------------------------------------------------------------- |
| `DrCeuticsQuiz.exe` | **Ứng dụng game**, chỉ một file duy nhất. Standee **không cần cài thêm gì** |
| `questions.json`    | Bộ câu hỏi, sửa được trực tiếp trên standee                                 |
| `logo.png`          | Logo                                                                        |

> Có thể có thêm vài file `.pdb`. Đó là file dùng để dò lỗi, xoá đi cũng không sao.

**Đây là toàn bộ những gì cần mang sang standee.** Hãy nén thư mục `win-x64` thành file `.zip` để chép bằng USB cho tiện.

---

## 7. Cài và vận hành trên standee

### 7.1. Chép game lên standee

1. Chép thư mục `win-x64` (hoặc file `.zip` rồi giải nén) vào standee, ví dụ đặt tại `C:\DrCeuticsQuiz\`.
2. Nhấp đúp **`DrCeuticsQuiz.exe`** để mở game.
3. **Lần đầu mở**, Windows có thể hiện bảng xanh _"Windows protected your PC"_: bấm **More info** rồi bấm **Run anyway**. Bảng này hiện ra vì ứng dụng chưa được ký số, không phải virus.

### 7.2. Cài đặt màn hình standee

**Settings → System → Display:**

- **Display orientation:** chọn **Portrait** (màn hình dọc).
- **Scale:** chọn **100%**.

### 7.3. Bật máy là tự vào game

1. Nhấp chuột phải vào `DrCeuticsQuiz.exe` → **Show more options** → **Create shortcut**.
2. Nhấn phím **Windows + R**, gõ `shell:startup` rồi nhấn Enter. Một thư mục sẽ mở ra.
3. Kéo file shortcut vừa tạo vào thư mục đó.
4. Khởi động lại máy để kiểm tra.

Muốn tắt tính năng này thì xoá shortcut đó khỏi thư mục `shell:startup`.

### 7.4. Vận hành tại sự kiện

| Việc                                           | Cách làm                                                                 |
| ---------------------------------------------- | ------------------------------------------------------------------------ |
| Thoát game                                     | Cắm bàn phím, nhấn **Ctrl + Shift + Q**                                  |
| Xem số lượt chơi / số lượt nhận quà trong ngày | **Chạm logo DrCeutics 5 lần liên tiếp** (trong vòng 3 giây)              |
| Xoá số liệu thống kê                           | Mở bảng thống kê như trên → bấm **XOÁ SỐ LIỆU**                          |
| Đổi câu hỏi tại chỗ                            | Thoát game → sửa `questions.json` cạnh file `.exe` (mục 2) → mở lại game |
| Cập nhật bản build mới                         | Thoát game → chép đè các file mới vào thư mục cũ → mở lại                |

Game tự quay về màn chờ nếu không ai chạm màn hình trong 60 giây. Thống kê tự đếm lại từ 0 mỗi ngày mới.

---

## 8. Gặp lỗi thì làm gì

| Hiện tượng                                                                | Cách xử lý                                                                                              |
| ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| Gõ lệnh báo `dotnet: command not found` hoặc `'dotnet' is not recognized` | Chưa cài .NET 8 SDK (mục 4). Nếu đã cài thì **tắt hẳn VS Code / cửa sổ cmd rồi mở lại**                 |
| Game hiện câu _"Không đọc được questions.json…"_                          | File câu hỏi bị sai cú pháp. Xem lại [mục 2.6](#26-năm-lỗi-hay-gặp-nhất-game-sẽ-không-đọc-được-câu-hỏi) |
| Sửa câu hỏi trong project nhưng game trên standee không đổi               | Phải build lại (mục 6) rồi chép file mới sang. Hoặc sửa thẳng file `questions.json` trên standee        |
| Thay ảnh logo hoặc mascot nhưng không thấy đổi                            | Kiểm tra lại **tên file** (chữ thường, đuôi `.png`). Sau đó chạy lệnh `dotnet clean` rồi build lại      |
| Chữ hiện ra bị lỗi font, mất dấu                                          | Kiểm tra thư mục `Assets/Fonts/` còn đủ 3 file `MontExtraBold.ttf`, `MontBold.ttf`, `MontMedium.ttf`    |
| Game không full màn hình trên standee                                     | Đặt Scale = 100% (mục 7.2). Mở game bằng file `.exe`, không chạy với `--window`                         |
| Lỗi build khác                                                            | Chụp màn hình **toàn bộ dòng chữ đỏ** và gửi cho người hỗ trợ kỹ thuật                                  |
