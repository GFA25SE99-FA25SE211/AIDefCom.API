# Defense Report Service - H??ng d?n s? d?ng

## Mô t?

Service này t? ??ng t?o **Biên b?n b?o v? ?? án** t? transcript (b?n ghi phiên h?p) s? d?ng AI ?? phân tích n?i dung.

## Ki?n trúc

```
???????????????????
?  Transcript ID  ?
???????????????????
         ?
         ?
???????????????????????????????
?  DefenseReportService       ?
???????????????????????????????
? 1. L?y Transcript t? DB     ?
? 2. L?y DefenseSession       ?
? 3. L?y Council & Members    ?
? 4. L?y Group & Students     ?
? 5. G?i AI phân tích         ?
? 6. T?ng h?p báo cáo         ?
???????????????????????????????
         ?
         ?
???????????????????????????????
?  Defense Report Response    ?
???????????????????????????????
? • Council Info              ?
? • Session Info              ?
? • Project Info              ?
? • Defense Progress (AI)     ?
???????????????????????????????
```

## API Endpoint

### POST `/api/defense-reports/generate`

**Request Body:**
```json
{
  "transcriptId": 1
}
```

**Response:**
```json
{
  "code": 200,
  "message": "Defense report generated successfully",
  "data": {
    "councilInfo": {
      "councilId": 1,
      "majorName": "Software Engineering",
      "description": "H?i ??ng SE - Fall 2024",
      "members": [
        {
          "lecturerId": "TaiNT51",
          "fullName": "Nguy?n Tr?ng Tài",
          "role": "Ch? t?ch H?",
          "email": "TaiNT51@fe.edu.vn",
          "department": "Software Engineering",
          "academicRank": "Associate Professor",
          "degree": "Ph.D."
        }
      ]
    },
    "sessionInfo": {
      "defenseDate": "2025-10-05",
      "startTime": "15:22:00",
      "endTime": "16:55:00",
      "location": "P.408, Campus Khu CNC",
      "status": "Completed"
    },
    "projectInfo": {
      "projectCode": "SU25SE107",
      "topicTitleEN": "Pregnancy Care Companion",
      "topicTitleVN": "H? th?ng ch?m sóc thai k?",
      "semesterName": "Summer 2025",
      "year": 2025,
      "students": [
        {
          "studentId": "SE150712",
          "fullName": "?? H?u ??c",
          "email": "ducdhse150712@fpt.edu.vn",
          "groupRole": "Leader"
        }
      ]
    },
    "defenseProgress": {
      "actualStartTime": "15h22'",
      "actualEndTime": "16h55'",
      "studentPresentations": [
        {
          "studentName": "Nguyên",
          "presentationContent": [
            "Gi?i thi?u nhóm và ?? tài",
            "Actor & features",
            "Ki?n trúc h? th?ng",
            "Demo ch?c n?ng thai ph? t?o tài kho?n"
          ]
        },
        {
          "studentName": "Khiêm",
          "presentationContent": [
            "H? th?ng ?? xu?t ch? ?? dinh d??ng"
          ]
        }
      ],
      "questionsAndAnswers": [
        {
          "lecturer": "Cô Vân",
          "questions": [
            "Ch?c n?ng ??t l?ch khám ??nh k?: t? v?n viên có th? xem l?i n?i dung t? v?n, trao ??i v?i ng??i b?nh ? nh?ng l?n t? v?n tr??c ?ó không?",
            "H? th?ng có ph?n Dashboard th?ng kê không?",
            "Sequence diagrams trong tài li?u, ??u ???c v? ch?a ?úng."
          ],
          "answers": [
            "H? th?ng không có.",
            "Nhóm m? trang Dashboard => Nhóm nh?n góp ý c?a h?i ??ng.",
            "Nhóm nh?n thi?u sót."
          ]
        },
        {
          "lecturer": "Th?y Nhàn",
          "questions": [
            "H? th?ng có cho phép thai ph? có nhi?u l?n mang thai không?",
            "H? th?ng có b?n ?? t?ng tr??ng thai k? h?ng tu?n không?"
          ],
          "answers": [
            "H? th?ng có.",
            "Nhóm không tr? l?i ???c và nh?n góp ý c?a th?y."
          ]
        }
      ],
      "overallSummary": "Bu?i b?o v? di?n ra t? 15h22 ??n 16h55. Nhóm trình bày ?? tài Pregnancy Care Companion v?i các ch?c n?ng chính v? ch?m sóc thai k?. H?i ??ng ??a ra nhi?u câu h?i v? tính n?ng và thi?t k? h? th?ng.",
      "studentPerformance": "Sinh viên trình bày v?i s? phân công rõ ràng. Tuy nhiên, m?t s? câu h?i c?a h?i ??ng ch?a ???c tr? l?i ??y ??, ??c bi?t v? các tính n?ng nâng cao.",
      "discussionFocus": "Tính n?ng ??t l?ch khám, qu?n lý l?ch s? t? v?n, dashboard th?ng kê, và thi?t k? c? s? d? li?u cho nhi?u l?n mang thai."
    }
  }
}
```

## Cách s? d?ng

### 1. T? Frontend

```typescript
// TypeScript/JavaScript
const generateDefenseReport = async (transcriptId: number) => {
  const response = await fetch('http://localhost:5000/api/defense-reports/generate', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer YOUR_TOKEN'
    },
    body: JSON.stringify({ transcriptId })
  });
  
  const result = await response.json();
  return result.data;
};

// S? d?ng
const report = await generateDefenseReport(1);
console.log(report.councilInfo);
console.log(report.defenseProgress);
```

### 2. T? Postman/cURL

```bash
curl -X POST "http://localhost:5000/api/defense-reports/generate" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"transcriptId": 1}'
```

## C?u trúc d? li?u tr? v?

### 1. Council Info (Thông tin h?i ??ng)
- **councilId**: ID c?a h?i ??ng
- **majorName**: Tên ngành
- **description**: Mô t? h?i ??ng
- **members**: Danh sách thành viên h?i ??ng v?i ??y ?? thông tin (tên, ch?c v?, email, khoa, h?c hàm, h?c v?)

### 2. Session Info (Thông tin bu?i b?o v?)
- **defenseDate**: Ngày b?o v?
- **startTime**: Gi? b?t ??u (theo l?ch)
- **endTime**: Gi? k?t thúc (theo l?ch)
- **location**: ??a ?i?m
- **status**: Tr?ng thái (Scheduled, InProgress, Completed, etc.)

### 3. Project Info (Thông tin ?? tài)
- **projectCode**: Mã ?? tài
- **topicTitleEN**: Tên ?? tài ti?ng Anh
- **topicTitleVN**: Tên ?? tài ti?ng Vi?t
- **semesterName**: H?c k?
- **year**: N?m
- **students**: Danh sách sinh viên v?i vai trò (Leader/Member)

### 4. Defense Progress (Di?n bi?n - AI phân tích)
- **actualStartTime**: Gi? b?t ??u th?c t? (t? transcript)
- **actualEndTime**: Gi? k?t thúc th?c t? (t? transcript)
- **studentPresentations**: Ph?n trình bày c?a t?ng sinh viên
- **questionsAndAnswers**: Câu h?i t? h?i ??ng và câu tr? l?i
- **overallSummary**: Tóm t?t chung
- **studentPerformance**: ?ánh giá phong thái
- **discussionFocus**: Các ch? ?? tr?ng tâm

## AI Analysis

Service s? d?ng OpenRouter AI (GPT-4o-mini) ?? phân tích transcript và t? ??ng:

1. **Nh?n di?n ng??i trình bày**: Xác ??nh sinh viên nào trình bày ph?n nào
2. **Tóm t?t n?i dung**: Trích xu?t n?i dung chính c?a m?i ph?n
3. **Phân tích Q&A**: Nh?n di?n câu h?i t? gi?ng viên và câu tr? l?i t? sinh viên
4. **?ánh giá t?ng quan**: ??a ra nh?n xét v? bu?i b?o v?

## L?u ý

1. **Transcript ph?i có n?i dung**: Service s? throw error n?u transcript r?ng
2. **AI có th? không hoàn h?o**: N?u transcript không rõ ràng, AI có th? tr? v? "N/A" ho?c thông tin không chính xác
3. **Token limit**: Transcript quá dài (>8000 ký t?) s? b? c?t b?t ?? tránh v??t gi?i h?n token
4. **Fallback**: N?u AI fail, service s? tr? v? response v?i các tr??ng "N/A" thay vì throw error

## Error Handling

Service x? lý các l?i sau:

- **404**: Transcript/Session/Council/Group không t?n t?i
- **400**: Transcript không có n?i dung
- **500**: L?i AI ho?c l?i server

T?t c? errors ??u ???c log và tr? v? thông qua Global Exception Handler.

## Configuration

C?n c?u hình trong `appsettings.json`:

```json
{
  "AI": {
    "OpenRouterUrl": "https://openrouter.ai/api/v1/chat/completions",
    "OpenRouterToken": "sk-or-v1-...",
    "Model": "gpt-4o-mini"
  }
}
```

## Files Created

1. `AIDefCom.Service/Dto/DefenseReport/DefenseReportRequestDto.cs`
2. `AIDefCom.Service/Dto/DefenseReport/DefenseReportResponseDto.cs`
3. `AIDefCom.Service/Services/DefenseReportService/IDefenseReportService.cs`
4. `AIDefCom.Service/Services/DefenseReportService/DefenseReportService.cs`
5. `AIDefCom.API/Controllers/DefenseReportsController.cs`

## Testing

?? test service:

1. T?o m?t defense session v?i transcript
2. ??m b?o transcript có n?i dung
3. G?i API v?i transcript ID
4. Ki?m tra response có ??y ?? thông tin

```bash
# Test v?i transcript ID = 1
POST /api/defense-reports/generate
{
  "transcriptId": 1
}
```

---

**Tác gi?**: AIDefCom Development Team  
**Ngày t?o**: 2025  
**Version**: 1.0
