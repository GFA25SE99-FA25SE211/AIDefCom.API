namespace AIDefCom.Service.Constants
{
    /// <summary>
    /// Centralized system messages for consistent API responses
    /// All message codes follow the format: [Module].[Type][Code]
    /// </summary>
    public static class SystemMessages
    {
        // ==================== AUTHENTICATION & AUTHORIZATION ====================
        
        // Auth - Success Messages (0001-0099)
        public const string Auth_Success0001 = "Login successful.";
        public const string Auth_Success0002 = "User registered successfully.";
        public const string Auth_Success0003 = "Password changed successfully.";
        public const string Auth_Success0004 = "Token refreshed successfully.";
        public const string Auth_Success0005 = "Logout successful.";
        public const string Auth_Success0006 = "Password reset email sent successfully.";
        public const string Auth_Success0007 = "Password reset successfully.";
        public const string Auth_Success0008 = "Role assigned successfully.";
        public const string Auth_Success0009 = "Role created successfully.";
        public const string Auth_Success0010 = "Account restored successfully.";
        public const string Auth_Success0011 = "Account deactivated successfully.";
        public const string Auth_Success0012 = "Google login successful.";
        public const string Auth_Success0013 = "Password set successfully for Google account.";

        // Auth - Failure Messages (1001-1099)
        public const string Auth_Fail0001 = "Invalid email or password.";
        public const string Auth_Fail0002 = "User already exists.";
        public const string Auth_Fail0003 = "Invalid refresh token.";
        public const string Auth_Fail0004 = "Email claim not found in token.";
        public const string Auth_Fail0005 = "Current password is incorrect.";
        public const string Auth_Fail0006 = "Invalid authorization header.";
        public const string Auth_Fail0007 = "User not found.";
        public const string Auth_Fail0008 = "Registration failed.";

        // Auth - Validation Messages (2001-2099)
        public const string Auth_Validation0001 = "All fields (Email, Password, FullName, PhoneNumber) are required.";
        public const string Auth_Validation0002 = "Email and password cannot be empty.";
        public const string Auth_Validation0003 = "Email and role cannot be empty.";
        public const string Auth_Validation0004 = "Role name cannot be empty.";
        public const string Auth_Validation0005 = "All password fields are required.";
        public const string Auth_Validation0006 = "UserId and RefreshToken cannot be empty.";

        // ==================== COMMITTEE ASSIGNMENTS ====================
        
        // CommitteeAssignment - Success Messages (0001-0099)
        public const string CommitteeAssignment_Success0001 = "Committee assignments retrieved successfully.";
        public const string CommitteeAssignment_Success0002 = "Committee assignment retrieved successfully.";
        public const string CommitteeAssignment_Success0003 = "Committee assignment created successfully.";
        public const string CommitteeAssignment_Success0004 = "Committee assignment updated successfully.";
        public const string CommitteeAssignment_Success0005 = "Committee assignment deleted successfully.";
        public const string CommitteeAssignment_Success0006 = "Committee assignments by council retrieved successfully.";
        public const string CommitteeAssignment_Success0007 = "Committee assignments by session retrieved successfully.";
        public const string CommitteeAssignment_Success0008 = "Committee assignments by lecturer retrieved successfully.";

        // CommitteeAssignment - Failure Messages (1001-1099)
        public const string CommitteeAssignment_Fail0001 = "Committee assignment not found.";

        // ==================== COUNCILS ====================
        
        // Council - Success Messages (0001-0099)
        public const string Council_Success0001 = "Councils retrieved successfully.";
        public const string Council_Success0002 = "Council retrieved successfully.";
        public const string Council_Success0003 = "Council created successfully.";
        public const string Council_Success0004 = "Council updated successfully.";
        public const string Council_Success0005 = "Council deactivated successfully.";
        public const string Council_Success0006 = "Council restored successfully.";

        // Council - Failure Messages (1001-1099)
        public const string Council_Fail0001 = "Council not found.";
        public const string Council_Fail0002 = "Council not found or already active.";

        // ==================== DEFENSE SESSIONS ====================
        
        // DefenseSession - Success Messages (0001-0099)
        public const string DefenseSession_Success0001 = "Defense sessions retrieved successfully.";
        public const string DefenseSession_Success0002 = "Defense session retrieved successfully.";
        public const string DefenseSession_Success0003 = "Defense session created successfully.";
        public const string DefenseSession_Success0004 = "Defense session updated successfully.";
        public const string DefenseSession_Success0005 = "Defense session deleted successfully.";
        public const string DefenseSession_Success0006 = "Defense sessions by group retrieved successfully.";
        public const string DefenseSession_Success0007 = "Users retrieved successfully for the defense session.";

        // DefenseSession - Failure Messages (1001-1099)
        public const string DefenseSession_Fail0001 = "Defense session not found.";
        public const string DefenseSession_Fail0002 = "No users found for this defense session.";

        // ==================== STUDENTS ====================
        
        // Student - Success Messages (0001-0099)
        public const string Student_Success0001 = "Students retrieved successfully.";
        public const string Student_Success0002 = "Student retrieved successfully.";
        public const string Student_Success0003 = "Student created successfully.";
        public const string Student_Success0004 = "Student updated successfully.";
        public const string Student_Success0005 = "Student deleted successfully.";
        public const string Student_Success0006 = "Students by group retrieved successfully.";

        // Student - Failure Messages (1001-1099)
        public const string Student_Fail0001 = "Student not found.";

        // ==================== GROUPS ====================
        
        // Group - Success Messages (0001-0099)
        public const string Group_Success0001 = "Groups retrieved successfully.";
        public const string Group_Success0002 = "Group retrieved successfully.";
        public const string Group_Success0003 = "Group created successfully.";
        public const string Group_Success0004 = "Group updated successfully.";
        public const string Group_Success0005 = "Group deleted successfully.";
        public const string Group_Success0006 = "Groups by semester retrieved successfully.";

        // Group - Failure Messages (1001-1099)
        public const string Group_Fail0001 = "Group not found.";

        // ==================== MAJORS ====================
        
        // Major - Success Messages (0001-0099)
        public const string Major_Success0001 = "Majors retrieved successfully.";
        public const string Major_Success0002 = "Major retrieved successfully.";
        public const string Major_Success0003 = "Major created successfully.";
        public const string Major_Success0004 = "Major updated successfully.";
        public const string Major_Success0005 = "Major deleted successfully.";

        // Major - Failure Messages (1001-1099)
        public const string Major_Fail0001 = "Major not found.";

        // ==================== SEMESTERS ====================
        
        // Semester - Success Messages (0001-0099)
        public const string Semester_Success0001 = "Semesters retrieved successfully.";
        public const string Semester_Success0002 = "Semester retrieved successfully.";
        public const string Semester_Success0003 = "Semester created successfully.";
        public const string Semester_Success0004 = "Semester updated successfully.";
        public const string Semester_Success0005 = "Semester deleted successfully.";
        public const string Semester_Success0006 = "Semesters by major retrieved successfully.";

        // Semester - Failure Messages (1001-1099)
        public const string Semester_Fail0001 = "Semester not found.";

        // ==================== RUBRICS ====================
        
        // Rubric - Success Messages (0001-0099)
        public const string Rubric_Success0001 = "Rubrics retrieved successfully.";
        public const string Rubric_Success0002 = "Rubric retrieved successfully.";
        public const string Rubric_Success0003 = "Rubric created successfully.";
        public const string Rubric_Success0004 = "Rubric updated successfully.";
        public const string Rubric_Success0005 = "Rubric deleted successfully.";

        // Rubric - Failure Messages (1001-1099)
        public const string Rubric_Fail0001 = "Rubric not found.";

        // ==================== MAJOR-RUBRICS ====================
        
        // MajorRubric - Success Messages (0001-0099)
        public const string MajorRubric_Success0001 = "Major–Rubric links retrieved successfully.";
        public const string MajorRubric_Success0002 = "Rubrics for Major retrieved successfully.";
        public const string MajorRubric_Success0003 = "Majors for Rubric retrieved successfully.";
        public const string MajorRubric_Success0004 = "Major–Rubric link created successfully.";
        public const string MajorRubric_Success0005 = "Major–Rubric link updated successfully.";
        public const string MajorRubric_Success0006 = "Major–Rubric link deleted successfully.";

        // MajorRubric - Failure Messages (1001-1099)
        public const string MajorRubric_Fail0001 = "Major–Rubric link not found.";

        // ==================== PROJECT TASKS ====================
        
        // ProjectTask - Success Messages (0001-0099)
        public const string ProjectTask_Success0001 = "Project tasks retrieved successfully.";
        public const string ProjectTask_Success0002 = "Project task retrieved successfully.";
        public const string ProjectTask_Success0003 = "Project task created successfully.";
        public const string ProjectTask_Success0004 = "Project task updated successfully.";
        public const string ProjectTask_Success0005 = "Project task deleted successfully.";
        public const string ProjectTask_Success0006 = "Tasks assigned by user retrieved successfully.";
        public const string ProjectTask_Success0007 = "Tasks assigned to user retrieved successfully.";

        // ProjectTask - Failure Messages (1001-1099)
        public const string ProjectTask_Fail0001 = "Project task not found.";

        // ==================== REPORTS ====================
        
        // Report - Success Messages (0001-0099)
        public const string Report_Success0001 = "Reports retrieved successfully.";
        public const string Report_Success0002 = "Report retrieved successfully.";
        public const string Report_Success0003 = "Report created successfully.";
        public const string Report_Success0004 = "Report updated successfully.";
        public const string Report_Success0005 = "Report deleted successfully.";
        public const string Report_Success0006 = "Reports for session retrieved successfully.";

        // Report - Failure Messages (1001-1099)
        public const string Report_Fail0001 = "Report not found.";

        // ==================== TRANSCRIPTS ====================
        
        // Transcript - Success Messages (0001-0099)
        public const string Transcript_Success0001 = "Transcripts retrieved successfully.";
        public const string Transcript_Success0002 = "Transcript retrieved successfully.";
        public const string Transcript_Success0003 = "Transcript created successfully.";
        public const string Transcript_Success0004 = "Transcript updated successfully.";
        public const string Transcript_Success0005 = "Transcript deleted successfully.";

        // Transcript - Failure Messages (1001-1099)
        public const string Transcript_Fail0001 = "Transcript not found.";

        // ==================== TRANSCRIPT ANALYSIS ====================
        
        // TranscriptAnalysis - Success Messages (0001-0099)
        public const string TranscriptAnalysis_Success0001 = "Transcript analyzed successfully.";

        // ==================== RECORDINGS ====================
        
        // Recording - Success Messages (0001-0099)
        public const string Recording_Success0001 = "Upload initiated successfully.";
        public const string Recording_Success0002 = "Recording finalized successfully.";
        public const string Recording_Success0003 = "Read SAS URL generated successfully.";

        // Recording - Validation Messages (2001-2099)
        public const string Recording_Validation0001 = "UserId and MimeType are required.";
        public const string Recording_Validation0002 = "Request body is required.";
        public const string Recording_Validation0003 = "DurationSec and SizeBytes must be non-negative.";

        // ==================== MEMBER NOTES ====================
        
        // MemberNote - Success Messages (0001-0099)
        public const string MemberNote_Success0001 = "Member notes retrieved successfully.";
        public const string MemberNote_Success0002 = "Member note retrieved successfully.";
        public const string MemberNote_Success0003 = "Member note created successfully.";
        public const string MemberNote_Success0004 = "Member note updated successfully.";
        public const string MemberNote_Success0005 = "Member note deleted successfully.";
        public const string MemberNote_Success0006 = "Member notes by group retrieved successfully.";
        public const string MemberNote_Success0007 = "Member notes by user retrieved successfully.";

        // MemberNote - Failure Messages (1001-1099)
        public const string MemberNote_Fail0001 = "Member note not found.";

        // ==================== EMAIL ====================
        
        // Email - Success Messages (0001-0099)
        public const string Email_Success0001 = "OTP sent successfully.";
        public const string Email_Success0002 = "Email sent successfully.";
        public const string Email_Success0003 = "OTP verified successfully.";

        // ==================== USERS ====================
        
        // User - Success Messages (0001-0099)
        public const string User_Success0001 = "Users retrieved successfully.";
        public const string User_Success0002 = "User retrieved successfully.";

        // User - Failure Messages (1001-1099)
        public const string User_Fail0001 = "User not found.";

        // ==================== GENERAL VALIDATION ====================
        
        // General - Validation Messages (2001-2099)
        public const string General_Validation0001 = "Invalid input.";
        public const string General_Validation0002 = "Model validation failed.";
    }
}
