﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Institute_system
{
    public partial class studentF : Form
    {
        KeyValuePair<int, string> SelectedCourse;
        public static int SelectedExamID;

        public studentF()
        {
            InitializeComponent();
        }

        private void studCoursesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedCourse = (KeyValuePair<int, string>)studCoursesComboBox.SelectedItem;
       
            var ex = from es in appManager.entities.Exam_Student
                     join ee in appManager.entities.exams on es.Exam_ID equals ee.exam_ID
                     where ee.course_ID == SelectedCourse.Key
                     where es.St_ID == appManager.currentUser.stud_ID
                     where es.st_grade == null
                     select es;

            foreach (var EX in ex)
            {
                ExamscomboBox1.Items.Add(EX.Exam_ID);
            }
        }
        private void startExamBtn_Click(object sender, EventArgs e)
        {
            SelectedExamID = int.Parse(ExamscomboBox1.Text);
            appManager.examForm = new examF();
            appManager.examForm.Show();

        }


        private void updateInfoBtn_Click(object sender, EventArgs e)
        {
            string user = StudentUsernameTextBox.Text == "" ? appManager.currentUser.stud_Username : StudentUsernameTextBox.Text;
            string pass = StudentPasswordTextBox.Text == "" ? appManager.currentUser.stud_pw : StudentPasswordTextBox.Text;
            if (user == appManager.currentUser.stud_Username && pass == appManager.currentUser.stud_pw)
            {
                MessageBox.Show("You didn't enter new password or user name");
            }
            else
            {
                try
                {
                    appManager.entities.students_update(
                        appManager.currentUser.stud_ID, appManager.currentUser.stud_Fname,
                        appManager.currentUser.stud_Lname, appManager.currentUser.dept_ID,
                        user, pass);
                    MessageBox.Show("Credentials Updated Successfully");
                    appManager.entities = new sqlProjectEntities();
                }
                catch (Exception)
                {
                    MessageBox.Show("Username Already Exists");
                }
            }
        }

        //on form closing (sigout)
        private void studentF_FormClosing(object sender, FormClosingEventArgs e)
        {
            appManager.loginForm.Show();
        }

        private void StudentF_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Course Name");
            dt.Columns.Add("Exam ID");
            dt.Columns.Add("Grades");
           

            var studentgrades = from grd in appManager.entities.Exam_Student
                                join cour in appManager.entities.exams on grd.Exam_ID equals cour.exam_ID
                                join courName in appManager.entities.courses on cour.course_ID equals courName.c_ID
                                where grd.St_ID == appManager.currentUser.stud_ID
                                select new { courName.c_name,grd.Exam_ID,grd.st_grade } ;

            foreach (var item in studentgrades)
            {
                DataRow row = dt.NewRow();
                row["Course Name"] = item.c_name;
                row["Exam ID"] = item.Exam_ID; 
                row["Grades"] = item.st_grade;

                dt.Rows.Add(row);
            }
            dataGridView1.DataSource = dt;

            //Student_Exam Tab
            foreach(var c in appManager.currentUser.courses)
            {
                studCoursesComboBox.Items.Add(new KeyValuePair<int,string>(c.c_ID,c.c_name));
            }
            studCoursesComboBox.DisplayMember = "Value";
            studCoursesComboBox.ValueMember = "Key";
        }
    
        public void fillExamsTab()
        {
            //needs modificATION
            int id = appManager.currentUser.stud_ID;
            var stud = (from d in appManager.entities.students
                        where d.stud_ID == id
                        select d.courses);

            if (stud != null)
            {
                foreach (var course in stud)
                {
                    studCoursesComboBox.Items.Add(course);
                }
            }
        }

        
    }
}
