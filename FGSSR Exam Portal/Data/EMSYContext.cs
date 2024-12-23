using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using EMSYS.Models;

namespace EMSYS.Data
{

    public class EMSYSdbContext : IdentityDbContext<AspNetUsers, AspNetRoles, string,
                                        AspNetUserClaims, AspNetUserRoles, AspNetUserLogins, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public EMSYSdbContext(DbContextOptions<EMSYSdbContext> options)
            : base(options)
        {
        }

        //used by asp.net identity
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public DbSet<AspNetRoles> AspNetRoles { get; set; }
        public DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }

        //other tables
        public DbSet<GlobalOptionSet> GlobalOptionSets { get; set; }
        public DbSet<UserAttachment> UserAttachments { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ClassHub> ClassHubs { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<QuestionType> QuestionTypes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionAttachment> QuestionAttachments { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamClassHub> ExamClassHubs { get; set; }
        public DbSet<ExamSubject> ExamSubjects { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<StudentAnswerCloned> StudentAnswerCloneds { get; set; }
        public DbSet<StudentQuestionOrder> StudentQuestionOrders { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Country> Countries { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-MMBMKTC\MSSQLDEV;Initial Catalog=EMSYSDB;Persist Security Info=True;User ID=sa;Password=DB@dmin;TrustServerCertificate=True");
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.RemovePluralizingTableNameConvention();
            builder.Entity<AspNetUsers>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();
            base.OnModelCreating(builder);

            builder.Entity<ExamClassHub>().HasKey(m => new { m.ExamId, m.ClassHubId });
            builder.Entity<ExamQuestion>().HasKey(m => new { m.ExamId, m.QuestionId });
            builder.Entity<StudentClass>().HasKey(m => new { m.StudentId, m.ClassId });
            builder.Entity<ExamSubject>().HasKey(m => new { m.ExamId, m.SubjectId });

        }
    }
}