    create table Student
    (
        [studentId] int identity(1,1) not null, 
        [enrollmentDate] datetime null, 
        [classification] int not null, 
        [firstname]  varchar(100) not null, 
        [lastname] varchar(100) not null, 
      PRIMARY KEY CLUSTERED 
        (
    	[studentId] ASC
     )
)
GO 

create table Department
(
    [departmentId] int identity(1,1) not null, 
    [name] varchar(100) null, 
    [description] varchar(100) null, 
    primary key clustered 
    (
        [departmentId] ASC
    )
)
GO 

create table Course
(
    [courseId] int identity(1,1)  not null, 
    [departmentId] int not null, 
    [number] varchar(20) not null, 
    [name] varchar(100) not null, 
    [description] varchar (255) null,
    CONSTRAINT pk_course_id PRIMARY KEY CLUSTERED ([courseId])
)
GO 

create table Instructor
(
    [instructorId] int identity(1,1) not null, 
    [departmentId] int null, 
    [firstname] varchar(100) not null, 
    [lastname] varchar(100) not null
)
GO 

create table classification
(
    [classificationId] int not null,
    [description] varchar(100), 
    primary key clustered (
        [classificationId] ASC
    )
)
GO 

-- referential integrity
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[fk_student_has_classification]') AND parent_object_id = OBJECT_ID(N'[dbo].[Student]'))
ALTER TABLE [dbo].[Student]  WITH CHECK ADD  CONSTRAINT [fk_student_has_classification] FOREIGN KEY([studentId])
REFERENCES [dbo].[Classification] ([classificationId])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[fk_student_has_classification]') AND parent_object_id = OBJECT_ID(N'[dbo].[Student]'))
ALTER TABLE [dbo].[Student] CHECK CONSTRAINT [fk_student_has_classification]
GO

-- test data 
insert into classification (classificationId, [description]) values (0, 'Freshman')
GO 

insert into classification (classificationId, [description]) values (1, 'Sophmore')
GO

insert into classification (classificationId, [description]) values (2, 'Junior')
GO 

insert into classification (classificationId, [description]) values (3, 'Senior')
GO


