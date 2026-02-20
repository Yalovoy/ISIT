using DecisionTreeApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DecisionTreeApp.Services
{
    public class DecisionTreeService
    {
        private DecisionNode _rootNode;
        private DecisionNode _currentNode;
        private Stack<DecisionNode> _navigationHistory;
        private List<HistoryItem> _answerHistory;

        public event EventHandler<string> QuestionChanged;
        public event EventHandler<string> ResultReached;
        public event EventHandler<List<HistoryItem>> HistoryUpdated;

        public DecisionTreeService()
        {
            _navigationHistory = new Stack<DecisionNode>();
            _answerHistory = new List<HistoryItem>();
            BuildDecisionTree();
            ResetToRoot();
        }

        private void BuildDecisionTree()
        {
            var resultLaunchNow = new DecisionNode(101, "Результат", "Запускать немедленно");
            var resultNeedUSP = new DecisionNode(102, "Результат", "Нужно УТП (уникальное торговое предложение)");
            var resultAnalyzeCompetitors = new DecisionNode(103, "Результат", "Анализировать конкурентов");
            var resultFindNiche = new DecisionNode(104, "Результат", "Искать нишу");
            var resultLaunchCarefully = new DecisionNode(105, "Результат", "Запускать с осторожностью");
            var resultOptimizeProduction = new DecisionNode(106, "Результат", "Оптимизировать производство");
            var resultFindInvestors = new DecisionNode(107, "Результат", "Искать инвесторов");
            var resultReviseBusinessPlan = new DecisionNode(108, "Результат", "Пересмотреть бизнес-план");
            var resultPrepareLaunch = new DecisionNode(109, "Результат", "Готовиться к запуску");
            var resultPostponeLaunch = new DecisionNode(110, "Результат", "Отложить запуск");
            var resultDevelopOffSeason = new DecisionNode(111, "Результат", "Развивать в низкий сезон");
            var resultFindOtherMarket = new DecisionNode(112, "Результат", "Искать другой рынок");
            var resultAbandon = new DecisionNode(113, "Результат", "Отказаться от запуска");

            var nodeLowSeason = new DecisionNode(506, "Пик спроса не скоро?");
            nodeLowSeason.YesNode = resultDevelopOffSeason;
            nodeLowSeason.NoNode = resultFindOtherMarket;

            var nodePeakSoon = new DecisionNode(505, "Пик спроса скоро?");
            nodePeakSoon.YesNode = resultPrepareLaunch;
            nodePeakSoon.NoNode = resultPostponeLaunch;

            var nodeHighCost = new DecisionNode(504, "Высокая себестоимость?");
            nodeHighCost.YesNode = resultFindInvestors;
            nodeHighCost.NoNode = resultReviseBusinessPlan;

            var nodeLowCost = new DecisionNode(503, "Низкая себестоимость?");
            nodeLowCost.YesNode = resultLaunchCarefully;
            nodeLowCost.NoNode = resultOptimizeProduction;

            var nodeHighCompetition = new DecisionNode(502, "Высокая конкуренция?");
            nodeHighCompetition.YesNode = resultAnalyzeCompetitors;
            nodeHighCompetition.NoNode = resultFindNiche;

            var nodeLowCompetition = new DecisionNode(501, "Низкая конкуренция?");
            nodeLowCompetition.YesNode = resultLaunchNow;
            nodeLowCompetition.NoNode = resultNeedUSP;

            var nodeSeasonality = new DecisionNode(404, "Есть сезонность?");
            nodeSeasonality.YesNode = nodePeakSoon;
            nodeSeasonality.NoNode = nodeLowSeason;

            var nodeLowProfit = new DecisionNode(403, "Низкая прибыль?");
            nodeLowProfit.YesNode = nodeLowCost;
            nodeLowProfit.NoNode = nodeHighCost;

            var nodeHighProfit = new DecisionNode(402, "Высокая прибыль?");
            nodeHighProfit.YesNode = nodeLowCompetition;
            nodeHighProfit.NoNode = nodeHighCompetition;

            var nodeLowDemand = new DecisionNode(401, "Низкий спрос?");
            nodeLowDemand.YesNode = nodeSeasonality;
            nodeLowDemand.NoNode = resultAbandon;

            var nodeHighDemand = new DecisionNode(301, "Высокий спрос?");
            nodeHighDemand.YesNode = nodeHighProfit;
            nodeHighDemand.NoNode = nodeLowProfit;

            var root = new DecisionNode(1, "Запускать новый продукт?");
            root.YesNode = nodeHighDemand;
            root.NoNode = nodeLowDemand;

            _rootNode = root;
        }

        public void ResetToRoot()
        {
            _currentNode = _rootNode;
            _navigationHistory.Clear();
            _answerHistory.Clear();
            OnQuestionChanged(_currentNode.Text);
            OnHistoryUpdated();
        }

        public void AnswerYes()
        {
            if (_currentNode == null || _currentNode.IsLeaf) return;

            _answerHistory.Add(new HistoryItem
            {
                NodeId = _currentNode.Id,
                Question = _currentNode.Text,
                Answer = "Да",
                Time = DateTime.Now
            });

            _navigationHistory.Push(_currentNode);
            _currentNode = _currentNode.YesNode;

            if (_currentNode != null)
            {
                if (_currentNode.IsLeaf)
                    OnResultReached(_currentNode.Result);
                else
                    OnQuestionChanged(_currentNode.Text);
            }

            OnHistoryUpdated();
        }

        public void AnswerNo()
        {
            if (_currentNode == null || _currentNode.IsLeaf) return;

            _answerHistory.Add(new HistoryItem
            {
                NodeId = _currentNode.Id,
                Question = _currentNode.Text,
                Answer = "Нет",
                Time = DateTime.Now
            });

            _navigationHistory.Push(_currentNode);
            _currentNode = _currentNode.NoNode;

            if (_currentNode != null)
            {
                if (_currentNode.IsLeaf)
                    OnResultReached(_currentNode.Result);
                else
                    OnQuestionChanged(_currentNode.Text);
            }

            OnHistoryUpdated();
        }

        public bool GoBack()
        {
            if (_navigationHistory.Count > 0)
            {
                _currentNode = _navigationHistory.Pop();
                if (_answerHistory.Count > 0)
                    _answerHistory.RemoveAt(_answerHistory.Count - 1);

                OnQuestionChanged(_currentNode.Text);
                OnHistoryUpdated();
                return true;
            }
            return false;
        }

        public string GetCurrentQuestion() => _currentNode?.Text ?? "Дерево решений не инициализировано";
        public bool IsResultReached() => _currentNode != null && _currentNode.IsLeaf;
        public string GetCurrentResult() => _currentNode?.Result ?? "";
        public List<HistoryItem> GetHistory() => _answerHistory;

        public TreeNode GetTreeViewNodes()
        {
            return BuildTreeNode(_rootNode);
        }

        private TreeNode BuildTreeNode(DecisionNode node)
        {
            if (node == null) return null;

            var treeNode = new TreeNode(node.Text);
            treeNode.Tag = node.Id;

            if (node.IsLeaf)
            {
                treeNode.Text = node.Result; 
            }

            if (node.YesNode != null)
            {
                var yesNode = BuildTreeNode(node.YesNode);
                yesNode.Text = "Да → " + (yesNode.Text.Contains("Результат") ? "" : yesNode.Text);
                treeNode.Nodes.Add(yesNode);
            }

            if (node.NoNode != null)
            {
                var noNode = BuildTreeNode(node.NoNode);
                noNode.Text = "Нет → " + (noNode.Text.Contains("Результат") ? "" : noNode.Text);
                treeNode.Nodes.Add(noNode);
            }

            return treeNode;
        }

        public void HighlightCurrentNode(TreeNodeCollection nodes, int currentNodeId)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && (int)node.Tag == currentNodeId)
                {
                    node.BackColor = SystemColors.Highlight;
                    node.ForeColor = SystemColors.HighlightText;
                }
                else
                {
                    node.BackColor = SystemColors.Window;
                    node.ForeColor = SystemColors.WindowText;
                }

                if (node.Nodes.Count > 0)
                    HighlightCurrentNode(node.Nodes, currentNodeId);
            }
        }

        protected virtual void OnQuestionChanged(string question) => QuestionChanged?.Invoke(this, question);
        protected virtual void OnResultReached(string result) => ResultReached?.Invoke(this, result);
        protected virtual void OnHistoryUpdated() => HistoryUpdated?.Invoke(this, _answerHistory);
    }
}