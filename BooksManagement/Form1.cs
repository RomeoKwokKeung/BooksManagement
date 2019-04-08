using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BooksManagement
{
    public partial class frmAuthors : Form
    {
        public frmAuthors()
        {
            InitializeComponent();
        }

        OleDbConnection booksConn;
        OleDbCommand authorsComm;
        OleDbDataAdapter authorsAdapter;
        DataTable authorsTable;
        CurrencyManager authorsManager;
        OleDbCommandBuilder builderComm;
        bool dbError = false;
        public string AppState { get; set; }
        public int CurrentPosition { get; set; }

        private void frmAuthors_Load(object sender, EventArgs e)
        {
            try
            {
                var connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\DB\Books.accdb;
                    Persist Security Info = False;";

                booksConn = new OleDbConnection(connString);
                booksConn.Open();
                authorsComm = new OleDbCommand("SELECT * from Authors Order By Author", booksConn);
                authorsAdapter = new OleDbDataAdapter();
                authorsTable = new DataTable();
                authorsAdapter.SelectCommand = authorsComm;
                authorsAdapter.Fill(authorsTable);

                txtAuthorID.DataBindings.Add("Text", authorsTable, "AU_ID");
                txtAuthorName.DataBindings.Add("Text", authorsTable, "Author");
                txtYearBorn.DataBindings.Add("Text", authorsTable, "Year_Born");
                // seems like array
                authorsManager = (CurrencyManager)BindingContext[authorsTable];
                SetAppState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dbError = true;
            }

        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            authorsManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            authorsManager.Position++;
        }

        private void frmClosing(object sender, FormClosingEventArgs e)
        {
            if (!dbError)
            {
                booksConn.Close();
                booksConn.Dispose();
                authorsComm.Dispose();
                authorsAdapter.Dispose();
                authorsTable.Dispose();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                var savedRecord = txtAuthorName.Text;
                //changed
                authorsManager.EndCurrentEdit();
                builderComm = new OleDbCommandBuilder(authorsAdapter);
                if (AppState == "Edit")
                {
                    var authRow = authorsTable.Select("Au_ID = " + txtAuthorID.Text);
                    if (String.IsNullOrEmpty(txtYearBorn.Text))
                    {
                        authRow[0]["Year_Born"] = DBNull.Value;
                    }
                    else
                    {
                        authRow[0]["Year_Born"] = txtYearBorn.Text;
                    }
                    //update datatable
                    authorsAdapter.Update(authorsTable);
                    txtYearBorn.DataBindings.Add("Text", authorsTable, "Year_Born");
                }
                else
                {
                    //reflesh and sort list before display
                    authorsTable.DefaultView.Sort = "Author";
                    //find the lastest one                   
                    authorsManager.Position = authorsTable.DefaultView.Find(savedRecord);
                    authorsAdapter.Update(authorsTable);
                }

                MessageBox.Show("Record Save", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetAppState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dbError = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (response == DialogResult.No)
            {
                return;
            }

            try
            {
                authorsManager.RemoveAt(authorsManager.Position);
                builderComm = new OleDbCommandBuilder(authorsAdapter);
                authorsAdapter.Update(authorsTable);
                AppState = "Delete";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Deleting Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetAppState(string appState)
        {
            switch (appState)
            {
                case "View":
                    txtAuthorName.ReadOnly = true;
                    txtYearBorn.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAddNew.Enabled = true;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    txtAuthorName.TabStop = false;
                    txtYearBorn.TabStop = false;
                    btnFirst.Enabled = true;
                    btnLast.Enabled = true;
                    btnSearch.Enabled = true;
                    txtSearch.Enabled = true;
                    break;
                default: //add and edit state
                    txtAuthorName.ReadOnly = false;
                    txtYearBorn.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAddNew.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    btnFirst.Enabled = false;
                    btnLast.Enabled = false;
                    btnSearch.Enabled = false;
                    txtSearch.Enabled = false;
                    txtAuthorName.Focus();
                    break;
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentPosition = authorsManager.Position;
                authorsManager.AddNew();
                SetAppState("Add");
                AppState = "Add";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Adding Record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            // fix the bug
            txtYearBorn.DataBindings.Clear();
            SetAppState("Edit");
            AppState = "Edit";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            authorsManager.CancelCurrentEdit();
            if (AppState == "Edit")
            {
                txtYearBorn.DataBindings.Add("Text", authorsTable, "Year_Born");
            }

            if (AppState == "Add")
            {
                authorsManager.Position = CurrentPosition;
            }
            SetAppState("View");
        }

        private void txtAuthorBorn_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == 8)
            {
                e.Handled = false;
                lblWrongInput.Visible = false;
            }
            else
            {
                e.Handled = true;
                lblWrongInput.Visible = true;
            }
        }

        private bool ValidateInput()
        {
            string message = "";
            int inputYear, currentYear;
            bool allOK = true;

            if (txtAuthorName.Text.Trim().Equals(""))
            {
                message = "Author's name is required" + "\r\n";
                txtAuthorName.Focus();
                allOK = false;
            }

            if (!txtYearBorn.Text.Trim().Equals(""))
            {
                inputYear = Convert.ToInt32(txtYearBorn.Text);
                currentYear = DateTime.Now.Year;
                if (inputYear >= currentYear)
                {
                    message += "Invalid Year";
                    txtYearBorn.Focus();
                    allOK = false;
                }
            }

            if (!allOK)
            {
                MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return allOK;
        }

        private void txtAuthorName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                txtYearBorn.Focus();
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            authorsManager.Position = 0;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            authorsManager.Position = authorsManager.Count - 1;
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Length < 3 || txtSearch.Text.Equals(""))
            {
                MessageBox.Show("Invalid search", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            DataRow[] foundRecord;
            authorsTable.DefaultView.Sort = "Author";
            foundRecord = authorsTable.Select("Author LIKE '*" + txtSearch.Text + "*'");

            if (foundRecord.Length == 0)
            {
                MessageBox.Show("Nothing was found", "Nothing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                frmSearch searchForm = new frmSearch(foundRecord, "Authors");
                searchForm.ShowDialog();
                var index = searchForm.Index;
                authorsManager.Position = authorsTable.DefaultView.Find(foundRecord[index]["Author"]);
            }
        }
    }
}
