namespace AIDefCom.Service.Constants
{
    /// <summary>
    /// Centralized message codes for API responses
    /// Format: [Module].[Type][Code]
    /// Type: Success (0xxx), Fail (1xxx), Validation (2xxx), Warning (3xxx)
    /// </summary>
    public static class MessageCodes
    {
        // ==================== AUTHENTICATION & AUTHORIZATION ====================
        
        // Auth - Success
        public const string Auth_Success0001 = "Auth.Success0001";
        public const string Auth_Success0002 = "Auth.Success0002";
        public const string Auth_Success0003 = "Auth.Success0003";
        public const string Auth_Success0004 = "Auth.Success0004";
        public const string Auth_Success0005 = "Auth.Success0005";
        public const string Auth_Success0006 = "Auth.Success0006";
        public const string Auth_Success0007 = "Auth.Success0007";
        public const string Auth_Success0008 = "Auth.Success0008";
        public const string Auth_Success0009 = "Auth.Success0009";
        public const string Auth_Success0010 = "Auth.Success0010";
        public const string Auth_Success0011 = "Auth.Success0011";
        public const string Auth_Success0012 = "Auth.Success0012";
        public const string Auth_Success0013 = "Auth.Success0013";

        // Auth - Failure
        public const string Auth_Fail0001 = "Auth.Fail0001";
        public const string Auth_Fail0002 = "Auth.Fail0002";
        public const string Auth_Fail0003 = "Auth.Fail0003";
        public const string Auth_Fail0004 = "Auth.Fail0004";
        public const string Auth_Fail0005 = "Auth.Fail0005";
        public const string Auth_Fail0006 = "Auth.Fail0006";
        public const string Auth_Fail0007 = "Auth.Fail0007";
        public const string Auth_Fail0008 = "Auth.Fail0008";

        // Auth - Validation
        public const string Auth_Validation0001 = "Auth.Validation0001";
        public const string Auth_Validation0002 = "Auth.Validation0002";
        public const string Auth_Validation0003 = "Auth.Validation0003";
        public const string Auth_Validation0004 = "Auth.Validation0004";
        public const string Auth_Validation0005 = "Auth.Validation0005";
        public const string Auth_Validation0006 = "Auth.Validation0006";

        // ==================== COMMITTEE ASSIGNMENTS ====================
        
        public const string CommitteeAssignment_Success0001 = "CommitteeAssignment.Success0001";
        public const string CommitteeAssignment_Success0002 = "CommitteeAssignment.Success0002";
        public const string CommitteeAssignment_Success0003 = "CommitteeAssignment.Success0003";
        public const string CommitteeAssignment_Success0004 = "CommitteeAssignment.Success0004";
        public const string CommitteeAssignment_Success0005 = "CommitteeAssignment.Success0005";
        public const string CommitteeAssignment_Success0006 = "CommitteeAssignment.Success0006";
        public const string CommitteeAssignment_Success0007 = "CommitteeAssignment.Success0007";
        public const string CommitteeAssignment_Success0008 = "CommitteeAssignment.Success0008";
        public const string CommitteeAssignment_Fail0001 = "CommitteeAssignment.Fail0001";

        // ==================== COUNCILS ====================
        
        public const string Council_Success0001 = "Council.Success0001";
        public const string Council_Success0002 = "Council.Success0002";
        public const string Council_Success0003 = "Council.Success0003";
        public const string Council_Success0004 = "Council.Success0004";
        public const string Council_Success0005 = "Council.Success0005";
        public const string Council_Success0006 = "Council.Success0006";
        public const string Council_Fail0001 = "Council.Fail0001";
        public const string Council_Fail0002 = "Council.Fail0002";

        // ==================== DEFENSE SESSIONS ====================
        
        public const string DefenseSession_Success0001 = "DefenseSession.Success0001";
        public const string DefenseSession_Success0002 = "DefenseSession.Success0002";
        public const string DefenseSession_Success0003 = "DefenseSession.Success0003";
        public const string DefenseSession_Success0004 = "DefenseSession.Success0004";
        public const string DefenseSession_Success0005 = "DefenseSession.Success0005";
        public const string DefenseSession_Success0006 = "DefenseSession.Success0006";
        public const string DefenseSession_Success0007 = "DefenseSession.Success0007";
        public const string DefenseSession_Fail0001 = "DefenseSession.Fail0001";
        public const string DefenseSession_Fail0002 = "DefenseSession.Fail0002";

        // ==================== STUDENTS ====================
        
        public const string Student_Success0001 = "Student.Success0001";
        public const string Student_Success0002 = "Student.Success0002";
        public const string Student_Success0003 = "Student.Success0003";
        public const string Student_Success0004 = "Student.Success0004";
        public const string Student_Success0005 = "Student.Success0005";
        public const string Student_Success0006 = "Student.Success0006";
        public const string Student_Fail0001 = "Student.Fail0001";

        // ==================== GROUPS ====================
        
        public const string Group_Success0001 = "Group.Success0001";
        public const string Group_Success0002 = "Group.Success0002";
        public const string Group_Success0003 = "Group.Success0003";
        public const string Group_Success0004 = "Group.Success0004";
        public const string Group_Success0005 = "Group.Success0005";
        public const string Group_Success0006 = "Group.Success0006";
        public const string Group_Fail0001 = "Group.Fail0001";

        // ==================== MAJORS ====================
        
        public const string Major_Success0001 = "Major.Success0001";
        public const string Major_Success0002 = "Major.Success0002";
        public const string Major_Success0003 = "Major.Success0003";
        public const string Major_Success0004 = "Major.Success0004";
        public const string Major_Success0005 = "Major.Success0005";
        public const string Major_Fail0001 = "Major.Fail0001";

        // ==================== SEMESTERS ====================
        
        public const string Semester_Success0001 = "Semester.Success0001";
        public const string Semester_Success0002 = "Semester.Success0002";
        public const string Semester_Success0003 = "Semester.Success0003";
        public const string Semester_Success0004 = "Semester.Success0004";
        public const string Semester_Success0005 = "Semester.Success0005";
        public const string Semester_Success0006 = "Semester.Success0006";
        public const string Semester_Fail0001 = "Semester.Fail0001";

        // ==================== RUBRICS ====================
        
        public const string Rubric_Success0001 = "Rubric.Success0001";
        public const string Rubric_Success0002 = "Rubric.Success0002";
        public const string Rubric_Success0003 = "Rubric.Success0003";
        public const string Rubric_Success0004 = "Rubric.Success0004";
        public const string Rubric_Success0005 = "Rubric.Success0005";
        public const string Rubric_Fail0001 = "Rubric.Fail0001";

        // ==================== MAJOR-RUBRICS ====================
        
        public const string MajorRubric_Success0001 = "MajorRubric.Success0001";
        public const string MajorRubric_Success0002 = "MajorRubric.Success0002";
        public const string MajorRubric_Success0003 = "MajorRubric.Success0003";
        public const string MajorRubric_Success0004 = "MajorRubric.Success0004";
        public const string MajorRubric_Success0005 = "MajorRubric.Success0005";
        public const string MajorRubric_Success0006 = "MajorRubric.Success0006";
        public const string MajorRubric_Fail0001 = "MajorRubric.Fail0001";

        // ==================== PROJECT TASKS ====================
        
        public const string ProjectTask_Success0001 = "ProjectTask.Success0001";
        public const string ProjectTask_Success0002 = "ProjectTask.Success0002";
        public const string ProjectTask_Success0003 = "ProjectTask.Success0003";
        public const string ProjectTask_Success0004 = "ProjectTask.Success0004";
        public const string ProjectTask_Success0005 = "ProjectTask.Success0005";
        public const string ProjectTask_Success0006 = "ProjectTask.Success0006";
        public const string ProjectTask_Success0007 = "ProjectTask.Success0007";
        public const string ProjectTask_Fail0001 = "ProjectTask.Fail0001";

        // ==================== REPORTS ====================
        
        public const string Report_Success0001 = "Report.Success0001";
        public const string Report_Success0002 = "Report.Success0002";
        public const string Report_Success0003 = "Report.Success0003";
        public const string Report_Success0004 = "Report.Success0004";
        public const string Report_Success0005 = "Report.Success0005";
        public const string Report_Success0006 = "Report.Success0006";
        public const string Report_Fail0001 = "Report.Fail0001";

        // ==================== TRANSCRIPTS ====================
        
        public const string Transcript_Success0001 = "Transcript.Success0001";
        public const string Transcript_Success0002 = "Transcript.Success0002";
        public const string Transcript_Success0003 = "Transcript.Success0003";
        public const string Transcript_Success0004 = "Transcript.Success0004";
        public const string Transcript_Success0005 = "Transcript.Success0005";
        public const string Transcript_Fail0001 = "Transcript.Fail0001";

        // ==================== TRANSCRIPT ANALYSIS ====================
        
        public const string TranscriptAnalysis_Success0001 = "TranscriptAnalysis.Success0001";

        // ==================== RECORDINGS ====================
        
        public const string Recording_Success0001 = "Recording.Success0001";
        public const string Recording_Success0002 = "Recording.Success0002";
        public const string Recording_Success0003 = "Recording.Success0003";
        public const string Recording_Validation0001 = "Recording.Validation0001";
        public const string Recording_Validation0002 = "Recording.Validation0002";
        public const string Recording_Validation0003 = "Recording.Validation0003";

        // ==================== MEMBER NOTES ====================
        
        public const string MemberNote_Success0001 = "MemberNote.Success0001";
        public const string MemberNote_Success0002 = "MemberNote.Success0002";
        public const string MemberNote_Success0003 = "MemberNote.Success0003";
        public const string MemberNote_Success0004 = "MemberNote.Success0004";
        public const string MemberNote_Success0005 = "MemberNote.Success0005";
        public const string MemberNote_Success0006 = "MemberNote.Success0006";
        public const string MemberNote_Success0007 = "MemberNote.Success0007";
        public const string MemberNote_Fail0001 = "MemberNote.Fail0001";

        // ==================== EMAIL ====================
        
        public const string Email_Success0001 = "Email.Success0001";
        public const string Email_Success0002 = "Email.Success0002";
        public const string Email_Success0003 = "Email.Success0003";

        // ==================== USERS ====================
        
        public const string User_Success0001 = "User.Success0001";
        public const string User_Success0002 = "User.Success0002";
        public const string User_Fail0001 = "User.Fail0001";

        // ==================== GENERAL ====================
        
        public const string General_Validation0001 = "General.Validation0001";
        public const string General_Validation0002 = "General.Validation0002";
    }
}
