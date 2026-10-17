## Giới thiệu về [AXIS](https://axis.prioris.jp/)

AXIS là dịch vụ phát thông tin động đất và thời tiết theo thời gian thực bằng WebSocket. (miễn phí)  
Mặc định, KyoshinEewViewer for ingen nhận thông tin động đất bằng [XML phòng chống thiên tai của JMA (kiểu PULL)](http://xml.kishou.go.jp/xmlpull.html), nhưng cách này có độ trễ **khoảng 1 đến 3 phút** từ lúc công bố đến lúc nhận.  
Dùng AXIS có thể giảm xuống **vài giây**.

### Các kênh hiện được hỗ trợ

- `eew`
    - Dùng được ở tab Kyoshin Monitor.
    - Trạng thái cảnh báo có thể chưa ổn định.
- Thông tin động đất, thông tin sóng thần, v.v. dự kiến sẽ được hỗ trợ sau.

### Lưu ý

- AXIS đang trong giai đoạn thử nghiệm beta và độ ổn định cũng chưa được kiểm chứng trong ứng dụng này. Có thể xảy ra lỗi ngoài dự kiến.
- AXIS không nhận được thông tin quá khứ, nên không nhận được các thông tin chưa được phản ánh trong nguồn cấp của JMA.
- Chỉ được dùng dịch vụ cho mục đích phi thương mại.

> [!WARNING]
> Nghiêm cấm phân phối lại, đăng lại, sao chép, chỉnh sửa và sử dụng thương mại thông tin nhận được từ AXIS.
> Nếu bạn quay hoặc phát video, hãy hết sức lưu ý.
