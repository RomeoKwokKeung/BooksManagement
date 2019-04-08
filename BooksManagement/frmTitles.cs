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
    public partial class frmTitles : Form
    {
        public frmTitles()
        {
            InitializeComponent();
        }

        OleDbConnection booksConn;
        OleDbCommand titlesComm;
        OleDbDataAdapter titlesAdapter;
        DataTable titlesTable;
        CurrencyManager titlesManager;
        public string appState { get; set; }
        OleDbCommandBuilder builderComm;
        public int currentPosition { get; set; }
        OleDbCommand authorsComm;
        OleDbDataAdapter authorsAdapter;
        DataTable[] authorsTable = new DataTable[4];
        ComboBox[] authorCombo = new ComboBox[4];
        OleDbCommand ISBNAuthorsComm;
        OleDbDataAdapter ISBNAuthorsAdapter;
        DataTable ISBNAuthorsTable;
        OleDbCommand publisherComm;
        OleDbDataAdapter publisherAdaptor;
        DataTable publisherTable;


        private void frmTitles_Load(object sender, EventArgs e)
        {
            try
            {
                var connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\DB\Books.accdb;
                                Persist Security Info=False;";
                booksConn = new OleDbConnection(connString);
                booksConn.Open();
                titlesComm = new OleDbCommand("SELECT * FROM Titles ORDER BY Title", booksConn);
                titlesAdapter = new OleDbDataAdapter();
                titlesAdapter.SelectCommand = titlesComm;
                titlesTable = new DataTable();
                titlesAdapter.Fill(titlesTable);
                txtTitle.DataBindings.Add("Text", titlesTable, "Title");
                txtYearPublished.DataBindings.Add("Text", titlesTable, "Year_Published");
                txtISBN.DataBindings.Add("Text", titlesTable, "ISBN");
                txtDescription.DataBindings.Add("Text", titlesTable, "Description");
                txtNotes.DataBindings.Add("Text", titlesTable, "Notes");
                txtSubject.DataBindings.Add("Text", titlesTable, "Subject");
                txtComments.DataBindings.Add("Text", titlesTable, "Commets");
                titlesManager = (CurrencyManager)BindingContext[titlesTable];

                // show author name in combo box
                authorCombo[0] = cboAuthor1;
                authorCombo[1] = cboAuthor2;
                authorCombo[2] = cboAuthor3;
                authorCombo[3] = cboAuthor4;
                authorsComm = new OleDbCommand("SELECT * FROM Authors ORDER BY Author", booksConn);
                authorsAdapter = new OleDbDataAdapter();
                authorsAdapter.SelectCommand = authorsComm;

                for (int i = 0; i < 4; i++)
                {
                    authorsTable[i] = new DataTable();
                    authorsAdapter.Fill(authorsTable[i]);
                    authorCombo[i].DataSource = authorsTable[i];
                    authorCombo[i].DisplayMember = "Author";
                    authorCombo[i].ValueMember = "Au_ID";
                    authorCombo[i].SelectedIndex = -1;
                }

                // show publisher name in combo box
                publisherComm = new OleDbCommand("Select * from Publishers Order By Name", booksConn);
                publisherAdaptor = new OleDbDataAdapter();
                publisherTable = new DataTable();
                publisherAdaptor.SelectCommand = publisherComm;
                publisherAdaptor.Fill(publisherTable);

                cboPublisher.DataSource = publisherTable;
                cboPublisher.DisplayMember = "Name";
                cboPublisher.ValueMember = "PubID";
                cboPublisher.DataBindings.Add("SelectedValue", titlesTable, "PubID");


                SetAppState("View");
                GetAuthors();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmClosing(object sender, FormClosingEventArgs e)
        {
            booksConn.Close();
            booksConn.Dispose();
            titlesComm.Dispose();
            titlesAdapter.Dispose();
            titlesTable.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            titlesManager.Position = 0;
            GetAuthors();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            titlesManager.Position = titlesManager.Count - 1;
            GetAuthors();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            titlesManager.Position--;
            GetAuthors();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            titlesManager.Position++;
            GetAuthors();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtSearch.Text.Equals("") || txtSearch.Text.Length < 3)
            {
                MessageBox.Show("Invalid Search", "Invalid Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow[] foundRecords;
            titlesTable.DefaultView.Sort = "Title";
            foundRecords = titlesTable.Select("Title LIKE '*" + txtSearch.Text + "*'");

            if (foundRecords.Length == 0)
            {
                MessageBox.Show("No record Found", "No record Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                frmSearch searchForm = new frmSearch(foundRecords, "Titles");
                searchForm.ShowDialog();
                var index = searchForm.Index;  
                titlesManager.Position = titlesTable.DefaultView.Find(foundRecords[index]["Title"]);
                GetAuthors();
            }
        }

        private void SetAppState(string appState)
        {
            switch (appState)
            {
                case "View":
                    txtTitle.ReadOnly = true;
                    txtYearPublished.ReadOnly = true;
                    txtISBN.ReadOnly = true;
                    txtDescription.ReadOnly = true;
                    txtNotes.ReadOnly = true;
                    txtSubject.ReadOnly = true;
                    txtComments.ReadOnly = true;
                    button1.Enabled = true; //btnFirst
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAddNew.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    btnAuthors.Enabled = true;
                    btnPublishers.Enabled = true;
                    cboAuthor1.Enabled = false;
                    cboAuthor2.Enabled = false;
                    cboAuthor3.Enabled = false;
                    cboAuthor4.Enabled = false;
                    btnXAuthor1.Enabled = false;
                    btnXAuthor2.Enabled = false;
                    btnXAuthor3.Enabled = false;
                    btnXAuthor4.Enabled = false;
                    cboPublisher.Enabled = false;
                    break;
                default:
                    txtTitle.ReadOnly = false;
                    txtYearPublished.ReadOnly = false;
                    if (appState == "Add")
                    {
                        txtISBN.ReadOnly = false;
                    }
                    else
                    {
                        txtISBN.ReadOnly = true;
                    }
                    txtDescription.ReadOnly = false;
                    txtNotes.ReadOnly = false;
                    txtSubject.ReadOnly = false;
                    txtComments.ReadOnly = false;
                    button1.Enabled = false; //btnFirst
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAddNew.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    btnAuthors.Enabled = false;
                    btnPublishers.Enabled = false;
                    cboAuthor1.Enabled = true;
                    cboAuthor2.Enabled = true;
                    cboAuthor3.Enabled = true;
                    cboAuthor4.Enabled = true;
                    btnXAuthor1.Enabled = true;
                    btnXAuthor2.Enabled = true;
                    btnXAuthor3.Enabled = true;
                    btnXAuthor4.Enabled = true;
                    cboPublisher.Enabled = true;
                    break;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            txtYearPublished.DataBindings.Clear();
            SetAppState("Edit");
            appState = "Edit";
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            currentPosition = titlesManager.Position;
            SetAppState("Add");
            titlesManager.AddNew();
            appState = "Add";
        }

        private bool ValidateInput()
        {
            string message = "";
            bool isOK = true;

            if (txtTitle.Text.Equals(""))
            {
                message = "You must enter a title.\r\n";
                isOK = false;
            }

            int inputYear, currentYear;
            if (!txtYearPublished.Text.Trim().Equals(""))
            {
                inputYear = Convert.ToInt32(txtYearPublished.Text);
                currentYear = DateTime.Now.Year;
                if (inputYear > currentYear)
                {
                    message += "Year published cannot bt greater than current year.\r\n";
                    isOK = false;
                }
            }

            if (!(txtISBN.Text.Length == 13))
            {
                message += "Incomplete ISBN.\r\n";
                isOK = false;
            }

            //TO DO validate publisher

            if (!isOK)
            {
                MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return isOK;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                var savedRecord = txtISBN.Text;
                titlesManager.EndCurrentEdit();
                builderComm = new OleDbCommandBuilder(titlesAdapter);

                if (appState == "Edit")
                {
                    var titleRow = titlesTable.Select("ISBN = '" + savedRecord + "'");

                    // no delete no editing
                    if (String.IsNullOrEmpty(txtYearPublished.Text))
                    {
                        titleRow[0]["Year_Published"] = DBNull.Value;
                    }
                    else
                    {
                        titleRow[0]["Year_Published"] = txtYearPublished.Text;
                    }
                    titlesAdapter.Update(titlesTable);
                    txtYearPublished.DataBindings.Add("Text", titlesTable, "Year_Published");
                }
                else
                {
                    titlesAdapter.Update(titlesTable);
                    DataRow[] foundRecords;
                    titlesTable.DefaultView.Sort = "Title";
                    // savedRecord = ISBN
                    foundRecords = titlesTable.Select("ISBN ='" + savedRecord + "'");
                    titlesManager.Position = titlesTable.DefaultView.Find(foundRecords[0]["Title"]);
                }

                builderComm = new OleDbCommandBuilder(ISBNAuthorsAdapter);
                if (ISBNAuthorsTable.Rows.Count != 0)
                {
                    for (int i = 0; i < ISBNAuthorsTable.Rows.Count; i++)
                    {
                        ISBNAuthorsTable.Rows[i].Delete();
                    }

                    ISBNAuthorsAdapter.Update(ISBNAuthorsTable);
                }

                for (int i = 0; i < 4; i++)
                {
                    if (authorCombo[i].SelectedIndex != -1)
                    {
                        ISBNAuthorsTable.Rows.Add();
                        ISBNAuthorsTable.Rows[ISBNAuthorsTable.Rows.Count - 1]["ISBN"] = txtISBN.Text;
                        ISBNAuthorsTable.Rows[ISBNAuthorsTable.Rows.Count - 1]["Au_ID"] = authorCombo[i].SelectedValue;
                    }
                }

                ISBNAuthorsAdapter.Update(ISBNAuthorsTable);

                MessageBox.Show("Record Saved", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetAppState("View");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure to delete?", "Delete record", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (response == DialogResult.No)
            {
                return;
            }

            try
            {
                titlesManager.RemoveAt(titlesManager.Position);
                builderComm = new OleDbCommandBuilder(titlesAdapter);
                titlesAdapter.Update(titlesTable);
                appState = "Delete";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error deleting", MessageBoxButtons.OK);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            titlesManager.CancelCurrentEdit();

            // bind textbox back
            if (appState == "Edit")
            {
                txtYearPublished.DataBindings.Add("Text", titlesTable, "Year_Published");
            }

            if (appState == "Add")
            {
                titlesManager.Position = currentPosition;
            }

            SetAppState("View");
        }

        private void txtYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == 8)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void GetAuthors()
        {
            // blank some authors
            for (int i = 0; i < 4; i++)
            {
                authorCombo[i].SelectedIndex = -1;
            }

            ISBNAuthorsComm = new OleDbCommand("Select * from Title_Author WHERE ISBN = '" + txtISBN.Text + "'", booksConn);
            ISBNAuthorsAdapter = new OleDbDataAdapter();
            ISBNAuthorsAdapter.SelectCommand = ISBNAuthorsComm;
            ISBNAuthorsTable = new DataTable();
            ISBNAuthorsAdapter.Fill(ISBNAuthorsTable);

            if (ISBNAuthorsTable.Rows.Count == 0)
            {
                // nothing to do
                return;
            }
            for (int i = 0; i < ISBNAuthorsTable.Rows.Count; i++)
            {
                authorCombo[i].SelectedValue = ISBNAuthorsTable.Rows[i]["Au_ID"].ToString();
            }
        }

        private void btnXAuthor_Click(object sender, EventArgs e)
        {
            Button btnclicked = (Button)sender;
            switch (btnclicked.Name)
            {
                case "btnXAuthor1":
                    cboAuthor1.SelectedIndex = -1;
                    break;
                case "btnXAuthor2":
                    cboAuthor2.SelectedIndex = -1;
                    break;
                case "btnXAuthor3":
                    cboAuthor3.SelectedIndex = -1;
                    break;
                case "btnXAuthor4":
                    cboAuthor4.SelectedIndex = -1;
                    break;
            }
        }

        private void btnAuthors_Click(object sender, EventArgs e)
        {
            frmAuthors authorForm = new frmAuthors();
            authorForm.ShowDialog();
            authorForm.Dispose();
            booksConn.Close();

            var connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\DB\Books.accdb;
                                Persist Security Info=False;";
            booksConn = new OleDbConnection(connString);
            booksConn.Open();
            authorsAdapter.SelectCommand = authorsComm;

            for (int i = 0; i < 4; i++)
            {
                authorsTable[i] = new DataTable();
                authorsAdapter.Fill(authorsTable[i]);
                authorCombo[i].DataSource = authorsTable[i];
            }

            // match ISBN display the form
            GetAuthors();
        }

        private void btnPublishers_Click(object sender, EventArgs e)
        {
            frmPublishers pubForm = new frmPublishers();
            pubForm.ShowDialog();
            pubForm.Dispose();
            booksConn.Close();

            var connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\DB\Books.accdb;
                                Persist Security Info=False;";
            booksConn = new OleDbConnection(connString);
            booksConn.Open();
            cboPublisher.DataBindings.Clear();
            publisherAdaptor.SelectCommand = publisherComm;
            publisherTable = new DataTable();
            publisherAdaptor.Fill(publisherTable);
            cboPublisher.DataSource = publisherTable;
            cboPublisher.DisplayMember = "Name";
            cboPublisher.ValueMember = "PubID";
            cboPublisher.DataBindings.Add("SelectedValue", titlesTable, "PubID");
        }
    }
}
