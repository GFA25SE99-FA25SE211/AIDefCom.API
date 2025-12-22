using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace AIDefCom.API.Swagger
{
    public class SchemaExampleFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == null) return;
            
            // Major DTOs
            if (context.Type.Name == "MajorCreateDto" || context.Type.Name == "MajorUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["majorName"] = new OpenApiString("Software Engineering"),
                    ["description"] = new OpenApiString("Focus on software development methodologies, programming, and system design")
                };
            }

            // MajorRubric DTOs
            if (context.Type.Name == "MajorRubricCreateDto" || context.Type.Name == "MajorRubricUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["majorId"] = new OpenApiInteger(1),
                    ["rubricId"] = new OpenApiInteger(1)
                };
            }

            // Rubric DTOs
            if (context.Type.Name == "RubricCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["rubricName"] = new OpenApiString("Presentation Skills"),
                    ["description"] = new OpenApiString("Ability to present ideas clearly and effectively"),
                    ["majorId"] = new OpenApiInteger(1)
                };
            }
            if (context.Type.Name == "RubricUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["rubricName"] = new OpenApiString("Presentation Skills (Updated)"),
                    ["description"] = new OpenApiString("Updated description")
                };
            }

            // Group DTOs
            if (context.Type.Name == "GroupCreateDto" || context.Type.Name == "GroupUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["projectCode"] = new OpenApiString("FA25SE135"),
                    ["topicTitle_EN"] = new OpenApiString("AI-Based Defense Committee Management System"),
                    ["topicTitle_VN"] = new OpenApiString("H? th?ng qu?n l� h?i ??ng b?o v? d?a tr�n AI"),
                    ["semesterId"] = new OpenApiInteger(1),
                    ["majorId"] = new OpenApiInteger(1),
                    ["status"] = new OpenApiString("Active")
                };
            }

            // Semester DTOs
            if (context.Type.Name == "SemesterCreateDto" || context.Type.Name == "SemesterUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["semesterName"] = new OpenApiString("Fall 2024"),
                    ["year"] = new OpenApiInteger(2024),
                    ["startDate"] = new OpenApiString("2024-09-01"),
                    ["endDate"] = new OpenApiString("2024-12-31")
                };
            }

            // Council DTOs
            if (context.Type.Name == "CouncilCreateDto" || context.Type.Name == "CouncilUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["majorId"] = new OpenApiInteger(1),
                    ["description"] = new OpenApiString("Software Engineering Defense Council - Fall 2024"),
                    ["isActive"] = new OpenApiBoolean(true)
                };
            }

            // DefenseSession DTOs
            if (context.Type.Name == "DefenseSessionCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["groupId"] = new OpenApiString("GFA25SE01"),
                    ["location"] = new OpenApiString("Room 601 - NVH Campus"),
                    ["defenseDate"] = new OpenApiString("2024-12-15"),
                    ["startTime"] = new OpenApiString("14:00:00"),
                    ["endTime"] = new OpenApiString("16:00:00"),
                    ["councilId"] = new OpenApiInteger(1)
                };
            }

            if (context.Type.Name == "DefenseSessionUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["groupId"] = new OpenApiString("GFA25SE01"),
                    ["location"] = new OpenApiString("Room 602 - NVH Campus"),
                    ["defenseDate"] = new OpenApiString("2024-12-16"),
                    ["startTime"] = new OpenApiString("09:00:00"),
                    ["endTime"] = new OpenApiString("11:00:00"),
                    ["status"] = new OpenApiString("Completed"),
                    ["councilId"] = new OpenApiInteger(1)
                };
            }

            // Account DTOs
            if (context.Type.Name == "LoginDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["email"] = new OpenApiString("admin@fpt.edu.vn"),
                    ["password"] = new OpenApiString("Admin@123")
                };
            }

            if (context.Type.Name == "CreateAccountDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["id"] = new OpenApiString("SE173501"),
                    ["fullName"] = new OpenApiString("Nguyen Van A"),
                    ["email"] = new OpenApiString("SE173501@fpt.edu.vn"),
                    ["phoneNumber"] = new OpenApiString("0123456789"),
                    ["password"] = new OpenApiString("SecurePass123!")
                };
            }

            if (context.Type.Name == "ChangePasswordDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["currentPassword"] = new OpenApiString("OldPass123!"),
                    ["newPassword"] = new OpenApiString("NewPass123!"),
                    ["confirmNewPassword"] = new OpenApiString("NewPass123!")
                };
            }

            if (context.Type.Name == "ForgotPasswordDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["email"] = new OpenApiString("john.doe@university.edu.vn")
                };
            }

            // Student DTOs
            if (context.Type.Name == "StudentCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["userId"] = new OpenApiString("SE173501"),
                    ["groupId"] = new OpenApiString("GFA25SE01"),
                    ["dateOfBirth"] = new OpenApiString("2002-01-15"),
                    ["gender"] = new OpenApiString("Male"),
                    ["role"] = new OpenApiString("Leader")
                };
            }

            if (context.Type.Name == "StudentUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["groupId"] = new OpenApiString("GFA25SE02"),
                    ["dateOfBirth"] = new OpenApiString("2002-01-15"),
                    ["gender"] = new OpenApiString("Male"),
                    ["role"] = new OpenApiString("Member")
                };
            }

            // Lecturer DTOs
            if (context.Type.Name == "LecturerCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["id"] = new OpenApiString("TaiNT51"),
                    ["fullName"] = new OpenApiString("Nguyen Trong Tai"),
                    ["email"] = new OpenApiString("TaiNT51@fe.edu.vn"),
                    ["phoneNumber"] = new OpenApiString("0987654321"),
                    ["dateOfBirth"] = new OpenApiString("1980-05-20"),
                    ["gender"] = new OpenApiString("Male"),
                    ["department"] = new OpenApiString("Software Engineering"),
                    ["academicRank"] = new OpenApiString("Associate Professor"),
                    ["degree"] = new OpenApiString("Ph.D.")
                };
            }

            if (context.Type.Name == "LecturerUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["fullName"] = new OpenApiString("Nguyen Trong Tai (Updated)"),
                    ["email"] = new OpenApiString("TaiNT51@fe.edu.vn"),
                    ["phoneNumber"] = new OpenApiString("0987654321"),
                    ["dateOfBirth"] = new OpenApiString("1980-05-20"),
                    ["gender"] = new OpenApiString("Male"),
                    ["department"] = new OpenApiString("Software Engineering"),
                    ["academicRank"] = new OpenApiString("Professor"),
                    ["degree"] = new OpenApiString("Ph.D.")
                };
            }

            // Score DTOs
            if (context.Type.Name == "ScoreCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["value"] = new OpenApiDouble(8.5),
                    ["rubricId"] = new OpenApiInteger(1),
                    ["evaluatorId"] = new OpenApiString("TaiNT51"),
                    ["studentId"] = new OpenApiString("SE173501"),
                    ["sessionId"] = new OpenApiInteger(1),
                    ["comment"] = new OpenApiString("Good presentation skills and technical knowledge")
                };
            }

            if (context.Type.Name == "ScoreUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["value"] = new OpenApiDouble(9.0),
                    ["comment"] = new OpenApiString("Excellent work with clear explanations and deep understanding")
                };
            }

            // CommitteeAssignment DTOs
            if (context.Type.Name == "CommitteeAssignmentCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["lecturerId"] = new OpenApiString("TaiNT51"),
                    ["councilId"] = new OpenApiInteger(1),
                    ["councilRoleId"] = new OpenApiInteger(1)
                };
            }

            if (context.Type.Name == "CommitteeAssignmentUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["lecturerId"] = new OpenApiString("HungLD5"),
                    ["councilId"] = new OpenApiInteger(1),
                    ["councilRoleId"] = new OpenApiInteger(2)
                };
            }

            // Transcript DTOs
            if (context.Type.Name == "TranscriptCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["sessionId"] = new OpenApiInteger(1),
                    ["transcriptText"] = new OpenApiString("This is the defense session transcript..."),
                    ["isApproved"] = new OpenApiBoolean(false)
                };
            }

            if (context.Type.Name == "TranscriptUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["transcriptText"] = new OpenApiString("Updated transcript with corrections..."),
                    ["isApproved"] = new OpenApiBoolean(true),
                    ["status"] = new OpenApiString("Approved")
                };
            }

            // ProjectTask DTOs
            if (context.Type.Name == "ProjectTaskCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["title"] = new OpenApiString("Review Project Documentation"),
                    ["description"] = new OpenApiString("Review and evaluate the project documentation before defense"),
                    ["assignedById"] = new OpenApiString("CA-001"),
                    ["assignedToId"] = new OpenApiString("CA-002"),
                    ["rubricId"] = new OpenApiInteger(1),
                    ["sessionId"] = new OpenApiInteger(1),
                    ["status"] = new OpenApiString("Pending")
                };
            }

            if (context.Type.Name == "ProjectTaskUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["title"] = new OpenApiString("Review Project Documentation (Updated)"),
                    ["description"] = new OpenApiString("Completed review with feedback provided"),
                    ["assignedById"] = new OpenApiString("CA-001"),
                    ["assignedToId"] = new OpenApiString("CA-002"),
                    ["rubricId"] = new OpenApiInteger(1),
                    ["sessionId"] = new OpenApiInteger(1),
                    ["status"] = new OpenApiString("Completed")
                };
            }

            // Report DTOs
            if (context.Type.Name == "ReportCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["sessionId"] = new OpenApiInteger(1),
                    ["filePath"] = new OpenApiString("/reports/defense-session-1-report.pdf"),
                    ["summaryText"] = new OpenApiString("Defense session completed successfully with all members present"),
                    ["status"] = new OpenApiString("Generated")
                };
            }

            if (context.Type.Name == "ReportUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["sessionId"] = new OpenApiInteger(1),
                    ["filePath"] = new OpenApiString("/reports/defense-session-1-report-updated.pdf"),
                    ["summaryText"] = new OpenApiString("Updated report with final scores and recommendations"),
                    ["status"] = new OpenApiString("Finalized")
                };
            }

            // MemberNote DTOs
            if (context.Type.Name == "MemberNoteCreateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["committeeAssignmentId"] = new OpenApiString("CA-001"),
                    ["sessionId"] = new OpenApiInteger(1),
                    ["noteContent"] = new OpenApiString("Student demonstrated strong technical knowledge but needs improvement in presentation skills")
                };
            }

            if (context.Type.Name == "MemberNoteUpdateDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["noteContent"] = new OpenApiString("Updated: Student showed significant improvement in final presentation. Recommended for high distinction.")
                };
            }
        }
    }
}
