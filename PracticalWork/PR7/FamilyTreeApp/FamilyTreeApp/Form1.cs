using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FamilyTreeApp.Models;
using FamilyTreeApp.Services;

namespace FamilyTreeApp
{
    public partial class Form1 : Form
    {
        private FamilyTreeService _familyService;
        private List<Person> _people;
        private List<DisplayRelationship> _relationships;

        private bool isAddingPerson = false;
        private bool isAddingRelationship = false;
        private Person selectedPersonForRelation = null;
        private Person selectedPersonForDelete = null; 

        private Stack<Action> undoStack = new Stack<Action>();

        public Form1()
        {
            InitializeComponent();
            InitializeService();
            UpdateDisplay();
            UpdateMenuStates();
        }

        private void InitializeService()
        {
            _familyService = new FamilyTreeService();
            _familyService.DataChanged += (s, e) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(UpdateDisplay));
                else
                    UpdateDisplay();
            };
        }

       

        private void UpdateDisplay()
        {
            _people = _familyService.GetAllPeople();
            _relationships = _familyService.GetDisplayRelationships();
            drawingPanel.Invalidate();

            UpdateMenuStates();
        }

        private void UpdateMenuStates()
        {
            bool hasPeople = _people != null && _people.Count > 0;

            deleteSelectedToolStripMenuItem.Enabled = selectedPersonForDelete != null;

            clearAllToolStripMenuItem.Enabled = hasPeople;

            addRelationshipToolStripMenuItem.Enabled = hasPeople && _people.Count >= 2;
        }

        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            DrawRelationships(g);
            DrawPeople(g);
            DrawModeHint(g);
            DrawSelectionHint(g);
        }

        private void DrawRelationships(Graphics g)
        {
            if (_relationships == null) return;

            foreach (var rel in _relationships)
            {
                Point from = new Point(rel.From.X, rel.From.Y);
                Point to = new Point(rel.To.X, rel.To.Y);

                using (Pen pen = new Pen(Color.Black, 2))
                {
                    if (rel.RelationType == "супруг")
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    g.DrawLine(pen, from, to);
                }
                DrawRelationshipLabel(g, rel, from, to);
            }
        }

        private void DrawRelationshipLabel(Graphics g, DisplayRelationship rel, Point from, Point to)
        {
            int midX = (from.X + to.X) / 2;
            int midY = (from.Y + to.Y) / 2;

            string labelText = "";
            if (rel.RelationType == "родитель")
                labelText = "родитель → ребенок";
            else if (rel.RelationType == "ребенок")
                labelText = "ребенок ← родитель";
            else if (rel.RelationType == "супруг")
                labelText = "супруг(а)";

            SizeF textSize = g.MeasureString(labelText, SystemFonts.DefaultFont);
            RectangleF textBg = new RectangleF(
                midX - textSize.Width / 2 - 2,
                midY - textSize.Height / 2 - 2,
                textSize.Width + 4,
                textSize.Height + 4
            );

            using (Brush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
            {
                g.FillRectangle(bgBrush, textBg);
            }
            using (Pen borderPen = new Pen(Color.LightGray, 1))
            {
                g.DrawRectangle(borderPen, textBg.X, textBg.Y, textBg.Width, textBg.Height);
            }

            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(labelText, SystemFonts.DefaultFont, textBrush,
                    midX - textSize.Width / 2, midY - textSize.Height / 2);
            }
        }

        private void DrawPeople(Graphics g)
        {
            if (_people == null) return;

            foreach (var person in _people)
            {
                SizeF textSize = g.MeasureString(person.Description, SystemFonts.DefaultFont);
                int width = (int)textSize.Width + 20;
                int height = (int)textSize.Height + 10;

                Rectangle rect = new Rectangle(
                    person.X - width / 2,
                    person.Y - height / 2,
                    width,
                    height
                );

                using (Brush brush = new SolidBrush(Color.LightBlue))
                {
                    g.FillRectangle(brush, rect);
                }

                using (Pen pen = new Pen(Color.Black, 1))
                {
                    g.DrawRectangle(pen, rect);
                }

                if (selectedPersonForDelete != null && selectedPersonForDelete.Id == person.Id)
                {
                    using (Pen pen = new Pen(Color.Red, 3))
                    {
                        g.DrawRectangle(pen, rect);
                    }

                    using (Brush brush = new SolidBrush(Color.Red))
                    {
                        g.DrawString("ВЫБРАНО", new Font("Arial", 8, FontStyle.Bold), brush,
                            rect.X, rect.Y - 15);
                    }
                }

                else if (selectedPersonForRelation != null && selectedPersonForRelation.Id == person.Id)
                {
                    using (Pen pen = new Pen(Color.Blue, 3))
                    {
                        g.DrawRectangle(pen, rect);
                    }
                }

                // Текст
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    g.DrawString(person.Description, SystemFonts.DefaultFont, brush,
                        rect.X + 10, rect.Y + 5);
                }
            }
        }

        private void DrawModeHint(Graphics g)
        {
            string hint = "";
            if (isAddingPerson)
                hint = "Режим: добавление человека. Щелкните по пустому месту.";
            else if (isAddingRelationship)
            {
                if (selectedPersonForRelation == null)
                    hint = "Режим: добавление связи. Выберите первого человека (родителя/супруга)...";
                else
                    hint = "Режим: добавление связи. Выберите второго человека (ребенка/супруга)...";
            }

            if (!string.IsNullOrEmpty(hint))
            {
                using (Font font = new Font("Arial", 10, FontStyle.Italic))
                using (Brush brush = new SolidBrush(Color.Gray))
                {
                    g.DrawString(hint, font, brush, 10, 50);
                }
            }
        }

        private void DrawSelectionHint(Graphics g)
        {
            if (selectedPersonForDelete != null)
            {
                string hint = "Нажмите 'Удалить выбранное' для удаления этого человека и всех его связей";
                using (Font font = new Font("Arial", 9, FontStyle.Italic))
                using (Brush brush = new SolidBrush(Color.Red))
                {
                    g.DrawString(hint, font, brush, 10, 70);
                }
            }
        }

        private void DrawingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (isAddingPerson)
            {
                AddNewPersonAtLocation(e.Location);
                isAddingPerson = false;
            }
            else if (isAddingRelationship)
            {
                HandleRelationshipClick(e.Location);
            }
            else
            {
                SelectPersonForDelete(e.Location);
            }

            drawingPanel.Invalidate();
        }

        private void HandleRelationshipClick(Point location)
        {
            Person clickedPerson = FindPersonAtLocation(location);

            if (clickedPerson != null)
            {
                if (selectedPersonForRelation == null)
                {
                    selectedPersonForRelation = clickedPerson;
                }
                else
                {
                    if (selectedPersonForRelation.Id == clickedPerson.Id)
                    {
                        MessageBox.Show("Нельзя создать связь с самим собой!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (RelationshipExists(selectedPersonForRelation.Id, clickedPerson.Id))
                    {
                        MessageBox.Show("Такая связь уже существует!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        selectedPersonForRelation = null;
                        return;
                    }

                    AddRelationshipBetween(selectedPersonForRelation, clickedPerson);

                    selectedPersonForRelation = null;
                }
            }
        }

        private bool RelationshipExists(int person1Id, int person2Id)
        {
            foreach (var rel in _relationships)
            {
                if ((rel.From.Id == person1Id && rel.To.Id == person2Id) ||
                    (rel.From.Id == person2Id && rel.To.Id == person1Id))
                {
                    return true;
                }
            }
            return false;
        }

        private void SelectPersonForDelete(Point location)
        {
            Person clickedPerson = FindPersonAtLocation(location);

            if (clickedPerson != null)
            {
                if (selectedPersonForDelete != null && selectedPersonForDelete.Id == clickedPerson.Id)
                {
                    selectedPersonForDelete = null;
                }
                else
                {
                    selectedPersonForDelete = clickedPerson;
                }

                UpdateMenuStates();
            }
        }

        private Person FindPersonAtLocation(Point location)
        {
            foreach (var person in _people)
            {
                int tolerance = 30;
                if (Math.Abs(person.X - location.X) < tolerance &&
                    Math.Abs(person.Y - location.Y) < tolerance)
                {
                    return person;
                }
            }
            return null;
        }

        private void AddNewPersonAtLocation(Point location)
        {
            using (var dialog = new AddPersonDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(dialog.FullName))
                    {
                        MessageBox.Show("Имя не может быть пустым!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dialog.BirthDate > DateTime.Now)
                    {
                        MessageBox.Show("Дата рождения не может быть в будущем!",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var person = _familyService.AddPerson(
                        dialog.FullName,
                        dialog.BirthDate
                    );

                    person.X = location.X;
                    person.Y = location.Y;

                    SaveUndoState(() => _familyService.DeletePerson(person.Id));

                    _familyService.NotifyDataChanged();
                }
            }
        }

        private void AddRelationshipBetween(Person person1, Person person2)
        {
            using (var dialog = new AddRelationshipDialog())
            {
                dialog.SetPersonInfo(person1.FullName, person2.FullName);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string relationType = dialog.RelationType;

                    if (!IsValidRelationship(person1, person2, relationType))
                    {
                        return;
                    }

                    SaveUndoState(() => {
                    });

                    if (relationType == "супруг")
                    {
                        _familyService.AddRelationship(person1.Id, person2.Id, "супруг");
                    }
                    else if (relationType == "родитель-ребенок")
                    {
                        _familyService.AddRelationship(person1.Id, person2.Id, "родитель");
                    }
                }
            }
        }

        private bool IsValidRelationship(Person person1, Person person2, string relationType)
        {
            if (RelationshipExists(person1.Id, person2.Id))
            {
                MessageBox.Show("Такая связь уже существует!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (relationType == "родитель-ребенок")
            {
                if (person1.BirthDate >= person2.BirthDate)
                {
                    var result = MessageBox.Show(
                        "Внимание: родитель младше ребенка или они одного возраста. Это возможно только для приемных родителей. Продолжить?",
                        "Проверка возраста",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    return result == DialogResult.Yes;
                }
            }

            return true;
        }

        private void SaveUndoState(Action undoAction)
        {
            undoStack.Push(undoAction);

            undoToolStripMenuItem.Enabled = true;
        }

        private void AddPersonMenuItem_Click(object sender, EventArgs e)
        {
            isAddingPerson = true;
            isAddingRelationship = false;
            selectedPersonForRelation = null;
            selectedPersonForDelete = null;
            UpdateMenuStates();
        }

        private void AddRelationshipMenuItem_Click(object sender, EventArgs e)
        {
            if (_people.Count < 2)
            {
                MessageBox.Show("Для создания связи нужно хотя бы 2 человека!",
                    "Недостаточно людей", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            isAddingPerson = false;
            isAddingRelationship = true;
            selectedPersonForRelation = null;
            selectedPersonForDelete = null;
            UpdateMenuStates();
        }

        private void DeleteSelectedMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedPersonForDelete == null)
            {
                MessageBox.Show("Сначала выберите человека для удаления (кликните по нему)",
                    "Нет выбранного объекта", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string message = $"Удалить {selectedPersonForDelete.FullName} и все связанные с ним связи?";
            if (MessageBox.Show(message, "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var deletedPerson = selectedPersonForDelete;
                SaveUndoState(() => {
                });

                _familyService.DeletePerson(selectedPersonForDelete.Id);
                selectedPersonForDelete = null;
                UpdateMenuStates();
            }
        }

        private void ClearAllMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Очистить всё дерево? Это действие нельзя отменить!",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                SaveUndoState(() => {
                });

                _familyService.ClearAll();
                isAddingPerson = false;
                isAddingRelationship = false;
                selectedPersonForRelation = null;
                selectedPersonForDelete = null;
                undoStack.Clear();
                undoToolStripMenuItem.Enabled = false;
                UpdateMenuStates();
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                var undoAction = undoStack.Pop();
                undoAction?.Invoke();

                if (undoStack.Count == 0)
                    undoToolStripMenuItem.Enabled = false;
            }
        }
    }

    public class AddPersonDialog : Form
    {
        public string FullName { get; private set; }
        public DateTime BirthDate { get; private set; }

        private TextBox txtName;
        private DateTimePicker dtpBirth;
        private Button btnOk;
        private Button btnCancel;

        public AddPersonDialog()
        {
            this.Text = "Добавить человека";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblName = new Label { Text = "ФИО:", Location = new Point(10, 20), Size = new Size(80, 20) };
            txtName = new TextBox { Location = new Point(100, 20), Size = new Size(150, 20) };

            Label lblBirth = new Label { Text = "Дата рождения:", Location = new Point(10, 50), Size = new Size(80, 20) };
            dtpBirth = new DateTimePicker
            {
                Location = new Point(100, 50),
                Size = new Size(150, 20),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Now,
                Value = DateTime.Now.AddYears(-30)
            };

            btnOk = new Button { Text = "OK", Location = new Point(60, 100), Size = new Size(75, 30), DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Отмена", Location = new Point(150, 100), Size = new Size(75, 30), DialogResult = DialogResult.Cancel };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblBirth, dtpBirth, btnOk, btnCancel });

            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите ФИО!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                FullName = txtName.Text.Trim();
                BirthDate = dtpBirth.Value;
            };
        }
    }

    public class AddRelationshipDialog : Form
    {
        public string RelationType { get; private set; }

        private ComboBox cmbType;
        private Button btnOk;
        private Button btnCancel;
        private Label lblPersonInfo;

        public AddRelationshipDialog()
        {
            this.Text = "Тип связи";
            this.Size = new Size(350, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblPersonInfo = new Label
            {
                Text = "Выберите тип связи:",
                Location = new Point(10, 10),
                Size = new Size(320, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblType = new Label { Text = "Тип связи:", Location = new Point(10, 40), Size = new Size(80, 20) };

            cmbType = new ComboBox
            {
                Location = new Point(100, 40),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbType.Items.AddRange(new string[] {
                "родитель-ребенок",
                "супруг"
            });
            cmbType.SelectedIndex = 0;

            btnOk = new Button { Text = "OK", Location = new Point(80, 80), Size = new Size(75, 30), DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Отмена", Location = new Point(180, 80), Size = new Size(75, 30), DialogResult = DialogResult.Cancel };

            this.Controls.AddRange(new Control[] { lblPersonInfo, lblType, cmbType, btnOk, btnCancel });

            btnOk.Click += (s, e) =>
            {
                RelationType = cmbType.SelectedItem.ToString();
            };
        }

        public void SetPersonInfo(string person1Name, string person2Name)
        {
            lblPersonInfo.Text = $"Связь между {person1Name} и {person2Name}";
        }
    }
}