﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Institute_system
{
    public partial class instructorF : Form
    {
        int questionNo = 10;
        instructor instructor;
        public instructorF()
        {
            InitializeComponent();
        }


        #region instructor form -> courses tab 

        int CurrentInst_ID = appManager.currentInstructor.inst_ID; //ID of current instructor
        KeyValuePair<int, string> SelectedCourse; //Selected course in courses tab

        /*******************fill topics of specific course in grid view************************/
        public void FillTopicsGrid()
        {
            appManager.entities = new sqlProjectEntities();
            cours C = appManager.entities.courses.Find(SelectedCourse.Key);
            topicsdatagrid.Rows.Clear();
            foreach (topic Topic in C.topics)
            {
                topicsdatagrid.Rows.Add(Topic.topic_ID.ToString(), Topic.topic_name);
            }
            if (topicsdatagrid.CurrentCell != null)
            {
                topicsdatagrid.CurrentCell.Selected = false;
            }
            TopicID.Text = TopicName.Text = string.Empty;
        }
        /*******************fill textboxes with selected course info**********************/
        public void FillCourseInfo(string Cour_name, int Cour_ID)
        {
            courseName.Text = Cour_name;
            CourseID.Text = Cour_ID.ToString();
        }
        /*******************fill combobox with courses names**********************/
        public void FillCoursesDropDown()
        {
            appManager.entities = new sqlProjectEntities();
            instructor = (from i in appManager.entities.instructors
                          where i.inst_ID == CurrentInst_ID
                          select i).First();
            coursesDropDown.Items.Clear();
            foreach (var course in instructor.courses)
            {
                coursesDropDown.Items.Add(new KeyValuePair<int, string>(course.c_ID, course.c_name));
            }
            coursesDropDown.ValueMember = "Key";
            coursesDropDown.DisplayMember = "Value";

            coursesDropDown.Text = string.Empty;
            topicsdatagrid.Rows.Clear();
            CourseID.Text = courseName.Text = string.Empty;
            TopicID.Text = TopicName.Text = string.Empty;
        }

        /*******************************************Events********************************************/

        /******************************Select course from combobox handler****************************/
        private void coursesDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedCourse = (KeyValuePair<int, string>)coursesDropDown.SelectedItem;
            FillTopicsGrid();
            FillCourseInfo(coursesDropDown.Text, SelectedCourse.Key);
        }
        /******************************Select specific row from gridview handler****************************/
        private void topicsdatagrid_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in topicsdatagrid.SelectedRows)
            {
                TopicID.Text = row.Cells[0].Value.ToString();
                TopicName.Text = row.Cells[1].Value.ToString();
            }
        }
        /******************************Update Course name by course ID****************************/
        private void updateCourseBtn_Click(object sender, EventArgs e)
        {
            int CID = int.Parse(CourseID.Text);
            var Course = (from c in appManager.entities.courses  //edit on stored procedure courses_select
                          where c.c_ID == CID
                          select c).First();
            if (Course != null)
            {
                Course.c_name = courseName.Text;
                appManager.entities.SaveChanges();
                FillCoursesDropDown();
                fillExamCourses(coursesComboBox1);
                fillExamCourses(coursesComboBox2);
                MessageBox.Show("Course Name is Updated");
            }
        }
        /******************************Insert Course****************************/
        private void insertCourseBtn_Click(object sender, EventArgs e)
        {
            int CID = int.Parse(CourseID.Text);
            string CName = courseName.Text.ToString();
            if (appManager.entities.courses_insert(CID, CName) == 1)
            {
                appManager.entities = new sqlProjectEntities();
                appManager.entities.ins_courseInsert(CurrentInst_ID, CID); //>>>>stored procedure de feha mo4kla
            }
            //appManager.entities.ins_courseInsert(CurrentInst_ID, CID); //Exception?????????????

            FillCoursesDropDown();
            fillExamCourses(coursesComboBox1);
            fillExamCourses(coursesComboBox2);
            MessageBox.Show("Course is inserted");

        }
        /******************************Delete course****************************/
        private void deleteCourseBtn_Click(object sender, EventArgs e)
        {
            appManager.entities.ins_courseDelete(CurrentInst_ID, int.Parse(CourseID.Text));
            FillCoursesDropDown();
            fillExamCourses(coursesComboBox1);
            fillExamCourses(coursesComboBox2);
            MessageBox.Show("Course is Deleted");
        }

        /******************************Update Topic name by course ID***************************/
        private void updateTopicBtn_Click(object sender, EventArgs e)
        {
            int TID = int.Parse(TopicID.Text);
            var Topic = (from t in appManager.entities.topics //edit topic_select procedure
                         where t.topic_ID == TID
                         select t).First();
            if (Topic != null)
            {
                Topic.topic_name = TopicName.Text;
                appManager.entities.SaveChanges();
                FillTopicsGrid();
                MessageBox.Show("Topic Name is Updated");
            }
        }
        /******************************Insert Topic****************************/
        private void insertTopicBtn_Click(object sender, EventArgs e)
        {
            appManager.entities.topic_insert(int.Parse(TopicID.Text), TopicName.Text.ToString(), SelectedCourse.Key);
            FillTopicsGrid();
            MessageBox.Show("Topic is inserted");
        }
        /******************************Delete Topic****************************/
        private void deleteTopicBtn_Click(object sender, EventArgs e)
        {
            appManager.entities.topic_delete(int.Parse(TopicID.Text));
            FillTopicsGrid();
            MessageBox.Show("Course is Deleted");
        }
        #endregion

        #region instructor form -> exams tab -> students grade tab
        //a function to fill the students grades tab on form load
        public void fillStudentsGradesTab()
        {
            //fill departments
            foreach (var dept in appManager.entities.departments)
            {
                dept_nameComboBox.Items.Add(dept.dept_name);
            }
            //fill students
            foreach (var student in appManager.entities.students)
            {
                studentsListBox.Items.Add($"{student.stud_ID}: {student.stud_Fname} {student.stud_Lname}");
            }
            //fill exams
            foreach (var exam in appManager.entities.Exam_Student)
            {
                examsIDsListBox.Items.Add($"{exam.Exam_ID}, {exam.exam.cours.c_name}");
            }
        }
        private void studentsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var studentID = int.Parse(studentsListBox.SelectedItem.ToString().Split(':')[0]);
            var studentName = studentsListBox.SelectedItem.ToString().Split(':')[1].Remove(0,1);
            //MessageBox.Show(studentID.ToString());
            examsIDsListBox.Items.Clear();
            foreach (var exam in appManager.entities.Exam_Student)
            {
                if (exam.St_ID == studentID)
                {
                    examsIDsListBox.Items.Add($"{exam.Exam_ID}, {exam.exam.cours.c_name}");
                }
            }
            stdNameTextBox.Text = studentName;
            examIDTextBox.Text = "";
            gradeTextBox.Text = "";
            correctExamBtn.Enabled = false;
        }
        private void examsIDsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (studentsListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Select a student first");
            }
            else
            {
                var studentID = int.Parse(studentsListBox.SelectedItem.ToString().Split(':')[0]);
                var examID = int.Parse(examsIDsListBox.SelectedItem.ToString().Split(',')[0]);
                var grade = appManager.entities.Exam_Student.Where(row => row.St_ID == studentID && row.Exam_ID == examID).Select(row => row.st_grade).First();
                examIDTextBox.Text = examID.ToString();
                gradeTextBox.Text = grade == null? "N/A yet" : grade.ToString();
                gradeTextBox.Text = grade == null? "N/A yet" : grade.ToString();
                correctExamBtn.Enabled = gradeTextBox.Text == "N/A yet"? true: false;
            }
        }
        private void correctExamBtn_Click(object sender, EventArgs e)
        {
            var studentID = int.Parse(studentsListBox.SelectedItem.ToString().Split(':')[0]);
            var retval = appManager.entities.correctExam(int.Parse(examIDTextBox.Text), studentID);
            if (retval == 1)
            {
                MessageBox.Show("exam is corrected successfully!");
                examsIDsListBox_SelectedIndexChanged(null, null);
            }
        }
        private void deptNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int deptID = 0;
            foreach (var dept in appManager.entities.departments)
            {
                if (dept.dept_name == dept_nameComboBox.SelectedItem.ToString())
                    deptID = dept.dept_ID;
            }
            studentsListBox.Items.Clear();
            foreach (var student in appManager.entities.students)
            {
                if (student.dept_ID == deptID)
                    studentsListBox.Items.Add($"{student.stud_ID}: {student.stud_Fname} {student.stud_Lname}");
            }
            examsIDsListBox.Items.Clear();
            foreach (var exam in appManager.entities.Exam_Student)
            {
                examsIDsListBox.Items.Add($"{exam.Exam_ID}, {exam.exam.cours.c_name}");
            }
        }
        #endregion

        #region instructor form -> exams tab -> editing exam db tab
        //a function to fill the editing exams tab on form load
        public void fillEditingExamTab()
        {
            coursesComboBox3.Items.Add("All courses");
            foreach (var course in appManager.entities.courses)
            {
                if (appManager.currentInstructor.courses.Contains(course))
                {
                    coursesComboBox3.Items.Add(course.c_name);
                }

            }
        }
        private void coursesComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (coursesComboBox3.SelectedIndex > 0)
            {
                int courseID = 0;
                //assign the course ID
                foreach (var course in appManager.entities.courses)
                {
                    if (course.c_name == coursesComboBox3.SelectedItem.ToString())
                        courseID = course.c_ID;
                }
                //populate the questions list box
                questionsListBox.Items.Clear();
                foreach (var question in appManager.entities.questions)
                {
                    if (question.c_ID == courseID)
                        questionsListBox.Items.Add($"{question.q_ID}: {question.q_desc}");
                }
            }
            else
            {
                questionsListBox.Items.Clear();
                foreach (var question in appManager.entities.questions)
                {
                    questionsListBox.Items.Add($"{question.q_ID}: {question.q_desc}");
                }
            }

        }
        private void questionsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //get the question ID
            int qID = int.Parse(questionsListBox.SelectedItem.ToString().Split(':')[0]);
            //retreive the question row
            var question = appManager.entities.questions.Find(qID);
            //asssign the form fields related to that question
            questionDescTextBox.Text = question.q_desc;
            choiceTypeComboBox.SelectedIndex = question.q_type == "mcq" ? 0 : 1;
            choice1_textBox.Text = question.choices.ElementAt(0).choice_desc;
            choice2_textBox.Text = question.choices.ElementAt(1).choice_desc;
            radioButton1.Checked = question.choices.ElementAt(0).isCorrect == "T" || question.choices.ElementAt(0).isCorrect == "t" ? true : false;
            radioButton2.Checked = question.choices.ElementAt(1).isCorrect == "T" || question.choices.ElementAt(1).isCorrect == "t" ? true : false;
            if (question.q_type == "mcq")
            {
                choice3_textBox.Text = question.choices.ElementAt(2).choice_desc;
                radioButton3.Checked = question.choices.ElementAt(2).isCorrect == "T" ? true : false;
            }
            else
            {
                choice3_textBox.Text = "";
                radioButton3.Checked = false;
            }
        }
        private void choiceTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (choiceTypeComboBox.SelectedIndex == 0)
            {
                choice1_textBox.Enabled = true;
                choice2_textBox.Enabled = true;
                choice3_textBox.Enabled = true;
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                radioButton3.Enabled = true;
            }
            else
            {
                choice1_textBox.Enabled = true;
                choice2_textBox.Enabled = true;
                choice3_textBox.Enabled = false;
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                radioButton3.Enabled = false;
            }
        }
        private void insertQBtn_Click(object sender, EventArgs e)
        {
            int courseID = 0;
            string qDesc = questionDescTextBox.Text;
            string qType = choiceTypeComboBox.SelectedItem.ToString();
            //assign the course ID
            foreach (var course in appManager.entities.courses)
            {
                if (course.c_name == coursesComboBox3.SelectedItem.ToString())
                    courseID = course.c_ID;
            }
            //appManager.entities.Questions_Insert();
            //--------------------------------------------------
            //insert the choices for the question
            int qIDEntered = 0;
            string choiceDesc = "";
            //string isCorrect = "F";
            //assign the question ID just entered
            foreach (var question in appManager.entities.questions)
            {
                if (question.q_desc == qDesc)
                    qIDEntered = question.q_ID;
            }
            int j = choiceTypeComboBox.SelectedItem.ToString() == "mcq" ? 3 : 2;
            string[] choices = { choice1_textBox.Text, choice2_textBox.Text, choice3_textBox.Text };
            RadioButton[] radioButtons = { radioButton1, radioButton2, radioButton3};
            for (int i = 0; i < j; i++)
            {
                choiceDesc = choices[i];
                if (radioButtons[i].Checked)
                {
                    //isCorrect = "T";
                }
                //appManager.entities.choices_insert();
            }

        }
        private void updateQBtn_Click(object sender, EventArgs e)
        {
            //we need update to take all parameters
            int courseID = 0;
            //assign the course ID
            foreach (var course in appManager.entities.courses)
            {
                if (course.c_name == coursesComboBox3.SelectedItem.ToString())
                    courseID = course.c_ID;
            }
            string qDesc = questionDescTextBox.Text;
            string qType = choiceTypeComboBox.SelectedItem.ToString();
            int qID = int.Parse(questionsListBox.SelectedItem.ToString().Split(':')[0]);
            //appManager.entities.Questions_Update()
        }
        private void deleteQBtn_Click(object sender, EventArgs e)
        {
            int qID = int.Parse(questionsListBox.SelectedItem.ToString().Split(':')[0]);
            appManager.entities.Questions_Delete(qID);
        }
        #endregion

        #region instructor form -> exams tab -> generating exam tab


        /*******************************Fill Exam Courses ComboBox******************************/
        private void fillExamCourses(ComboBox coursesComboBox)
        {
            appManager.entities = new sqlProjectEntities();
            //courses_select no arguments and selects c.ID or all
            coursesComboBox.Items.Clear();
            var courses = from c in instructor.courses
                          select c;
            foreach (var course in courses)
            {
                coursesComboBox.Items.Add(course.c_name);
            }

        }

        /*******************************Fill MCQ ComboBox*****************************/

        private void fillMcq(int qNo)
        {
            examMcqComboBox.Items.Clear();
            for (int i = 0; i <= qNo; i++)
            {
                examMcqComboBox.Items.Add(i);
            }

        }

        /*******************************Fill TF ComboBox******************************/

        private void fillTF(int qNo)
        {
            examTFComboBox.Items.Clear();
            for (int i = 0; i <= qNo; i++)
            {
                examTFComboBox.Items.Add(i);
            }
        }

        /******************************Fill Exam ID TextBox*****************************/

        private void fillExamIDTextBox()
        {
            //SP for exams_questions to select last generated exam
            var examID = (from ex in appManager.entities.exams_questions
                          select ex.exam_ID).OrderByDescending(ex => ex).First();

            examIDTextBox1.Text = examID.ToString();

        }

        /******************************Fill Exam ID ComboBox*****************************/

        private void fillExamIDComboBox()
        {
            examsIDComboBox1.Items.Clear();
            var courseID = (from cID in appManager.entities.courses
                            where cID.c_name == coursesComboBox2.Text
                            select cID.c_ID).First();

            var examID = from eID in appManager.entities.exams
                         where eID.course_ID == courseID
                         select eID.exam_ID;

            foreach (var exID in examID)
            {
                examsIDComboBox1.Items.Add(exID);
            }


        }

        /*******************************Fill Students Grid******************************/

        private void fillStudentsGridView()
        {
            studentsExamGridView.Rows.Clear();

            int id = (from cs in appManager.entities.courses
                      where cs.c_name == coursesComboBox2.Text
                      select cs.c_ID).First();

            var course = appManager.entities.courses.Find(id);


            foreach (student std in course.students)
            {
                studentsExamGridView.Rows.Add(false, std.stud_ID, std.stud_Fname + " " + std.stud_Lname);
            }

            if (studentsExamGridView.CurrentCell != null)
            {
                studentsExamGridView.CurrentCell.Selected = false;
            }

        }

        /**************************Check If Student Assigned*************************/

        private bool isStudentAssigned(int Studentid, int examID)
        {
            var stID = from std in appManager.entities.exams_questions
                       select std;

            foreach (var st in stID)
            {
                if (st.St_ID == Studentid && examID == st.exam_ID)
                {
                    return true;
                }
            }
            return false;
        }

        /*****************************Remove Assigned Students*****************************/

        private void removeAssignedStudents()
        {
            for (int i = 0; i < studentsExamGridView.RowCount; i++)
            {
                int stdID = int.Parse(studentsExamGridView.Rows[i].Cells[1].Value.ToString());
                int examsID = int.Parse(examsIDComboBox1.Text);

                if (isStudentAssigned(stdID, examsID))
                {
                    studentsExamGridView.Rows.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        /********************************Events*****************************************/

        /*****************************Exams MCQ Index Change****************************/

        private void ExamMcq_SelectedIndexChanged(object sender, EventArgs e)
        {

            int McqNo = int.Parse(examMcqComboBox.Text);
            int remQuestions = questionNo - McqNo;
            fillTF(remQuestions);
            examTFComboBox.Text = remQuestions.ToString();
            examTFComboBox.Enabled = false;

        }

        /*****************************Exam TF Index Change****************************/

        private void ExamTF_SelectedIndexChanged(object sender, EventArgs e)
        {

            int TFNo = int.Parse(examTFComboBox.Text);
            int remQuestions = questionNo - TFNo;
            //fillMcq(remQuestions);
            examMcqComboBox.Text = remQuestions.ToString();

        }

        /**************************Exams ID ComboBox Index Change*************************/
        private void CoursesComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillStudentsGridView();
            fillExamIDComboBox();
        }

        /**************************Exams ID ComboBox Index Change*************************/

        private void ExamsIDComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillStudentsGridView();
            removeAssignedStudents();
        }

        /*******************************Generate Exam Btn******************************/

        private void GenerateExamBtn_Click(object sender, EventArgs e)
        {
            string course = coursesComboBox1.Text;
            int McqNo = int.Parse(examMcqComboBox.Text);
            int TFNo = int.Parse(examTFComboBox.Text);
            appManager.entities.generateExam(course, TFNo, McqNo);
            MessageBox.Show("Exam Generated Successfully");

            fillExamIDTextBox();
        }


        /*******************************Assign Students Btn*******************************/

        private void assignStudentBtn_Click(object sender, EventArgs e)
        {
            int stdCount = 0;
            List<string> checkedStds = new List<string>();
            for (int i = 0; i < studentsExamGridView.RowCount; i++)
            {

                if (Convert.ToBoolean(studentsExamGridView.Rows[i].Cells[0].Value))
                {
                    checkedStds.Add(studentsExamGridView.Rows[i].Cells[1].Value.ToString());
                }

            }

            foreach (string checkedStd in checkedStds)
            {
                stdCount++;
                int examID = int.Parse(examsIDComboBox1.Text);

                appManager.entities.AssignExamStudent(examID, int.Parse(checkedStd));

                if (stdCount == checkedStds.Count)
                {
                    MessageBox.Show("Student assigned to exam successfully");
                }

            }
            checkedStds.Clear();
            removeAssignedStudents();

        }

        #endregion

        #region form -> update info tab
        private void button11_Click(object sender, EventArgs e)
        {
            int ID = appManager.currentInstructor.inst_ID;
            var inst = (from i in appManager.entities.instructors
                        where i.inst_ID == ID
                        select i).First();

            if (inst != null)
            {
                inst.inst_name = userNameTextBox.Text;
                inst.inst_ID = int.Parse(pwTextBox.Text);
                appManager.entities.SaveChanges();
                userNameTextBox.Text = pwTextBox.Text = string.Empty;
                MessageBox.Show("Your login Information has been updated successfully");
            }
        }
        #endregion
        
        #region Load and Close

        /*********************************Load Event************************************/
        private void instructorF_Load(object sender, EventArgs e)
        {

            //Exams Tab
            //Generate Exam Tab

            coursesComboBox1.Items.Clear();
            var courses = from c in appManager.entities.courses
                          select c;
            foreach (var course in courses)
            {
                coursesComboBox1.Items.Add(course.c_ID);
            }
            fillStudentsGradesTab();
            fillEditingExamTab();
            //Courses Tab
            FillCoursesDropDown();
            //Exam Generation Tab
            //Exam Generation
            fillExamCourses(coursesComboBox1);
            fillMcq(questionNo);
            fillTF(questionNo);
            //Students Assignment
            fillExamCourses(coursesComboBox2);
        }
        /****************************on form closing (signout)**************************/
        private void instructorF_FormClosing(object sender, FormClosingEventArgs e)
        {
            appManager.loginForm.Show();
            appManager.loginForm.pwTextBox.Text = "";
        }

        #endregion

        
    }
}