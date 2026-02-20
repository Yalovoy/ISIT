namespace DecisionTreeApp
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeViewDecision = new System.Windows.Forms.TreeView();
            this.panelQuestion = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnYes = new System.Windows.Forms.Button();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.listBoxHistory = new System.Windows.Forms.ListBox();
            this.panelResult = new System.Windows.Forms.Panel();
            this.lblResult = new System.Windows.Forms.Label();
            this.panelQuestion.SuspendLayout();
            this.panelResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewDecision
            // 
            this.treeViewDecision.Location = new System.Drawing.Point(0, 3);
            this.treeViewDecision.Name = "treeViewDecision";
            this.treeViewDecision.Size = new System.Drawing.Size(686, 617);
            this.treeViewDecision.TabIndex = 0;
            // 
            // panelQuestion
            // 
            this.panelQuestion.Controls.Add(this.btnReset);
            this.panelQuestion.Controls.Add(this.btnBack);
            this.panelQuestion.Controls.Add(this.btnNo);
            this.panelQuestion.Controls.Add(this.btnYes);
            this.panelQuestion.Controls.Add(this.lblQuestion);
            this.panelQuestion.Location = new System.Drawing.Point(692, 12);
            this.panelQuestion.Name = "panelQuestion";
            this.panelQuestion.Size = new System.Drawing.Size(459, 120);
            this.panelQuestion.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(316, 74);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(92, 36);
            this.btnReset.TabIndex = 4;
            this.btnReset.Text = "Сброс";
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(223, 74);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(92, 36);
            this.btnBack.TabIndex = 3;
            this.btnBack.Text = "Назад";
            this.btnBack.UseVisualStyleBackColor = true;
            // 
            // btnNo
            // 
            this.btnNo.Location = new System.Drawing.Point(153, 74);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(64, 36);
            this.btnNo.TabIndex = 2;
            this.btnNo.Text = "Нет";
            this.btnNo.UseVisualStyleBackColor = true;
            // 
            // btnYes
            // 
            this.btnYes.Location = new System.Drawing.Point(83, 74);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(67, 36);
            this.btnYes.TabIndex = 1;
            this.btnYes.Text = "Да";
            this.btnYes.UseVisualStyleBackColor = true;
            // 
            // lblQuestion
            // 
            this.lblQuestion.AutoSize = true;
            this.lblQuestion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.lblQuestion.Location = new System.Drawing.Point(86, 32);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(64, 22);
            this.lblQuestion.TabIndex = 0;
            this.lblQuestion.Text = "label1";
            // 
            // listBoxHistory
            // 
            this.listBoxHistory.FormattingEnabled = true;
            this.listBoxHistory.ItemHeight = 16;
            this.listBoxHistory.Location = new System.Drawing.Point(692, 224);
            this.listBoxHistory.Name = "listBoxHistory";
            this.listBoxHistory.Size = new System.Drawing.Size(459, 212);
            this.listBoxHistory.TabIndex = 2;
            // 
            // panelResult
            // 
            this.panelResult.Controls.Add(this.lblResult);
            this.panelResult.Location = new System.Drawing.Point(692, 138);
            this.panelResult.Name = "panelResult";
            this.panelResult.Size = new System.Drawing.Size(459, 80);
            this.panelResult.TabIndex = 2;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblResult.Location = new System.Drawing.Point(10, 10);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(64, 22);
            this.lblResult.TabIndex = 0;
            this.lblResult.Text = "label2";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 653);
            this.Controls.Add(this.panelResult);
            this.Controls.Add(this.listBoxHistory);
            this.Controls.Add(this.panelQuestion);
            this.Controls.Add(this.treeViewDecision);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panelQuestion.ResumeLayout(false);
            this.panelQuestion.PerformLayout();
            this.panelResult.ResumeLayout(false);
            this.panelResult.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewDecision;
        private System.Windows.Forms.Panel panelQuestion;
        private System.Windows.Forms.Label lblQuestion;
        private System.Windows.Forms.ListBox listBoxHistory;
        private System.Windows.Forms.Panel panelResult;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnYes;
    }
}

