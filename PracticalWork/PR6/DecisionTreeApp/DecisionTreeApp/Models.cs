using System;
using System.Collections.Generic;

namespace DecisionTreeApp.Models
{
    public class DecisionNode
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DecisionNode YesNode { get; set; }
        public DecisionNode NoNode { get; set; }
        public bool IsLeaf { get; set; }
        public string Result { get; set; }

        public DecisionNode(int id, string text)
        {
            Id = id;
            Text = text;
            IsLeaf = false;
        }

        public DecisionNode(int id, string text, string result)
        {
            Id = id;
            Text = text;
            Result = result;
            IsLeaf = true;
        }
    }

    public class HistoryItem
    {
        public int NodeId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"Вопрос: {Question} → Ответ: {Answer}";
        }
    }
}