using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DecisionTreeApp.Models;
using DecisionTreeApp.Services;

namespace DecisionTreeApp
{
    public partial class Form1 : Form
    {
        private DecisionTreeService _decisionService;
        private TreeNode _treeRoot;

        public Form1()
        {
            InitializeComponent();
            InitializeService();
            SetupForm();
            LoadDecisionTree();
            UpdateUI();
        }

        private void InitializeService()
        {
            _decisionService = new DecisionTreeService();

            _decisionService.QuestionChanged += (s, q) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => lblQuestion.Text = q));
                else
                    lblQuestion.Text = q;
            };

            _decisionService.ResultReached += (s, r) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        lblResult.Text = r;
                        btnYes.Enabled = false;
                        btnNo.Enabled = false;
                    }));
                }
                else
                {
                    lblResult.Text = r;
                    btnYes.Enabled = false;
                    btnNo.Enabled = false;
                }
            };

            _decisionService.HistoryUpdated += (s, history) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => UpdateHistoryList(history)));
                else
                    UpdateHistoryList(history);
            };
        }

        private void UpdateHistoryList(List<HistoryItem> history)
        {
            listBoxHistory.Items.Clear();
            foreach (var item in history)
            {
                listBoxHistory.Items.Add(item.ToString());
            }

            if (listBoxHistory.Items.Count > 0)
                listBoxHistory.TopIndex = listBoxHistory.Items.Count - 1;
        }

        

        private void SetupForm()
        {
            this.Text = "Дерево решений: Запуск нового продукта";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnYes.Click += BtnYes_Click;
            btnNo.Click += BtnNo_Click;
            btnBack.Click += BtnBack_Click;
            btnReset.Click += BtnReset_Click;
        }

        private void LoadDecisionTree()
        {
            treeViewDecision.Nodes.Clear();
            _treeRoot = _decisionService.GetTreeViewNodes();
            treeViewDecision.Nodes.Add(_treeRoot);
            treeViewDecision.ExpandAll();
        }

        private void UpdateUI()
        {
            lblQuestion.Text = _decisionService.GetCurrentQuestion();

            if (_decisionService.IsResultReached())
            {
                lblResult.Text = _decisionService.GetCurrentResult();
                btnYes.Enabled = false;
                btnNo.Enabled = false;
            }
            else
            {
                lblResult.Text = "";
                btnYes.Enabled = true;
                btnNo.Enabled = true;
            }

            if (treeViewDecision.Nodes.Count > 0)
                HighlightCurrentNode();
        }

        private void HighlightCurrentNode()
        {
            int currentNodeId = GetCurrentNodeId();
            if (currentNodeId > 0)
                _decisionService.HighlightCurrentNode(treeViewDecision.Nodes, currentNodeId);
        }

        private int GetCurrentNodeId()
        {
            string currentQuestion = _decisionService.GetCurrentQuestion();
            return FindNodeIdByText(treeViewDecision.Nodes, currentQuestion);
        }

        private int FindNodeIdByText(TreeNodeCollection nodes, string text)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text.Contains(text) && node.Tag != null)
                    return (int)node.Tag;

                if (node.Nodes.Count > 0)
                {
                    int result = FindNodeIdByText(node.Nodes, text);
                    if (result > 0) return result;
                }
            }
            return -1;
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            _decisionService.AnswerYes();
            UpdateUI();
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            _decisionService.AnswerNo();
            UpdateUI();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (!_decisionService.GoBack())
                MessageBox.Show("Вы уже в начале дерева решений", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateUI();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            _decisionService.ResetToRoot();
            listBoxHistory.Items.Clear(); 
            UpdateUI();
            btnYes.Enabled = true;
            btnNo.Enabled = true;
        }
    }
}