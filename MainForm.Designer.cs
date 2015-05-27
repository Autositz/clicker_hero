/*
 * Created by SharpDevelop.
 * User: Autositz
 * Date: 25/05/2015
 * Time: 01:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace clicker_hero
{
    partial class MainForm
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label labelElapsed;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxHeroLevel4;
        private System.Windows.Forms.CheckBox checkBoxHeroLevel3;
        private System.Windows.Forms.CheckBox checkBoxHeroLevel2;
        private System.Windows.Forms.CheckBox checkBoxHeroLevel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelTextClickInterval;
        private System.Windows.Forms.Label labelTextLastClicks;
        private System.Windows.Forms.GroupBox groupBoxClickables;
        private System.Windows.Forms.CheckBox checkBoxClickables;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonSetTimer;
        private System.Windows.Forms.Label labelTimerSet;
        private System.Windows.Forms.CheckBox checkBoxTimerActive;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelTextHeroAlign;
        
        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.labelElapsed = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxHeroLevel4 = new System.Windows.Forms.CheckBox();
            this.checkBoxHeroLevel3 = new System.Windows.Forms.CheckBox();
            this.checkBoxHeroLevel2 = new System.Windows.Forms.CheckBox();
            this.checkBoxHeroLevel1 = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxTimerActive = new System.Windows.Forms.CheckBox();
            this.buttonSetTimer = new System.Windows.Forms.Button();
            this.labelTimerSet = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.labelTextClickInterval = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.labelTextLastClicks = new System.Windows.Forms.Label();
            this.groupBoxClickables = new System.Windows.Forms.GroupBox();
            this.checkBoxClickables = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelTextHeroAlign = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxClickables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelElapsed
            // 
            this.labelElapsed.Location = new System.Drawing.Point(98, 20);
            this.labelElapsed.Name = "labelElapsed";
            this.labelElapsed.Size = new System.Drawing.Size(91, 23);
            this.labelElapsed.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxHeroLevel4);
            this.groupBox3.Controls.Add(this.checkBoxHeroLevel3);
            this.groupBox3.Controls.Add(this.checkBoxHeroLevel2);
            this.groupBox3.Controls.Add(this.checkBoxHeroLevel1);
            this.groupBox3.Location = new System.Drawing.Point(12, 118);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(100, 147);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Hero Levels";
            // 
            // checkBoxHeroLevel4
            // 
            this.checkBoxHeroLevel4.Location = new System.Drawing.Point(7, 113);
            this.checkBoxHeroLevel4.Name = "checkBoxHeroLevel4";
            this.checkBoxHeroLevel4.Size = new System.Drawing.Size(60, 24);
            this.checkBoxHeroLevel4.TabIndex = 3;
            this.checkBoxHeroLevel4.Text = "Slot 4";
            this.checkBoxHeroLevel4.UseVisualStyleBackColor = true;
            // 
            // checkBoxHeroLevel3
            // 
            this.checkBoxHeroLevel3.Location = new System.Drawing.Point(7, 82);
            this.checkBoxHeroLevel3.Name = "checkBoxHeroLevel3";
            this.checkBoxHeroLevel3.Size = new System.Drawing.Size(60, 24);
            this.checkBoxHeroLevel3.TabIndex = 2;
            this.checkBoxHeroLevel3.Text = "Slot 3";
            this.checkBoxHeroLevel3.UseVisualStyleBackColor = true;
            // 
            // checkBoxHeroLevel2
            // 
            this.checkBoxHeroLevel2.Location = new System.Drawing.Point(7, 51);
            this.checkBoxHeroLevel2.Name = "checkBoxHeroLevel2";
            this.checkBoxHeroLevel2.Size = new System.Drawing.Size(60, 24);
            this.checkBoxHeroLevel2.TabIndex = 1;
            this.checkBoxHeroLevel2.Text = "Slot 2";
            this.checkBoxHeroLevel2.UseVisualStyleBackColor = true;
            // 
            // checkBoxHeroLevel1
            // 
            this.checkBoxHeroLevel1.Location = new System.Drawing.Point(7, 20);
            this.checkBoxHeroLevel1.Name = "checkBoxHeroLevel1";
            this.checkBoxHeroLevel1.Size = new System.Drawing.Size(60, 24);
            this.checkBoxHeroLevel1.TabIndex = 0;
            this.checkBoxHeroLevel1.Text = "Slot 1";
            this.checkBoxHeroLevel1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxTimerActive);
            this.groupBox1.Controls.Add(this.buttonSetTimer);
            this.groupBox1.Controls.Add(this.labelTimerSet);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.labelTextClickInterval);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.labelTextLastClicks);
            this.groupBox1.Controls.Add(this.labelElapsed);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(335, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Timers";
            // 
            // checkBoxTimerActive
            // 
            this.checkBoxTimerActive.Checked = true;
            this.checkBoxTimerActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTimerActive.Location = new System.Drawing.Point(195, 15);
            this.checkBoxTimerActive.Name = "checkBoxTimerActive";
            this.checkBoxTimerActive.Size = new System.Drawing.Size(104, 24);
            this.checkBoxTimerActive.TabIndex = 6;
            this.checkBoxTimerActive.Text = "Timer active";
            this.checkBoxTimerActive.UseVisualStyleBackColor = true;
            this.checkBoxTimerActive.CheckedChanged += new System.EventHandler(this.StartStopTimer);
            // 
            // buttonSetTimer
            // 
            this.buttonSetTimer.Location = new System.Drawing.Point(98, 74);
            this.buttonSetTimer.Name = "buttonSetTimer";
            this.buttonSetTimer.Size = new System.Drawing.Size(80, 23);
            this.buttonSetTimer.TabIndex = 3;
            this.buttonSetTimer.Text = "Set Timer";
            this.buttonSetTimer.UseVisualStyleBackColor = true;
            this.buttonSetTimer.Click += new System.EventHandler(this.TimerChanged);
            // 
            // labelTimerSet
            // 
            this.labelTimerSet.Location = new System.Drawing.Point(7, 74);
            this.labelTimerSet.Name = "labelTimerSet";
            this.labelTimerSet.Size = new System.Drawing.Size(85, 23);
            this.labelTimerSet.TabIndex = 5;
            this.labelTimerSet.Text = "labelTimerSet";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(156, 47);
            this.textBox3.MaxLength = 2;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(23, 20);
            this.textBox3.TabIndex = 2;
            this.textBox3.Tag = "iTimerSeconds";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(98, 47);
            this.textBox1.MaxLength = 2;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(23, 20);
            this.textBox1.TabIndex = 0;
            // 
            // labelTextClickInterval
            // 
            this.labelTextClickInterval.Location = new System.Drawing.Point(7, 43);
            this.labelTextClickInterval.Name = "labelTextClickInterval";
            this.labelTextClickInterval.Size = new System.Drawing.Size(85, 27);
            this.labelTextClickInterval.TabIndex = 1;
            this.labelTextClickInterval.Text = "Timer setting\r\nHH:mm:ss";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(127, 47);
            this.textBox2.MaxLength = 2;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(23, 20);
            this.textBox2.TabIndex = 1;
            // 
            // labelTextLastClicks
            // 
            this.labelTextLastClicks.Location = new System.Drawing.Point(7, 20);
            this.labelTextLastClicks.Name = "labelTextLastClicks";
            this.labelTextLastClicks.Size = new System.Drawing.Size(85, 23);
            this.labelTextLastClicks.TabIndex = 0;
            this.labelTextLastClicks.Text = "Since last clicks";
            // 
            // groupBoxClickables
            // 
            this.groupBoxClickables.Controls.Add(this.checkBoxClickables);
            this.groupBoxClickables.Location = new System.Drawing.Point(118, 118);
            this.groupBoxClickables.Name = "groupBoxClickables";
            this.groupBoxClickables.Size = new System.Drawing.Size(118, 53);
            this.groupBoxClickables.TabIndex = 2;
            this.groupBoxClickables.TabStop = false;
            this.groupBoxClickables.Text = "Clickables";
            // 
            // checkBoxClickables
            // 
            this.checkBoxClickables.Checked = true;
            this.checkBoxClickables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxClickables.Location = new System.Drawing.Point(7, 20);
            this.checkBoxClickables.Name = "checkBoxClickables";
            this.checkBoxClickables.Size = new System.Drawing.Size(99, 24);
            this.checkBoxClickables.TabIndex = 0;
            this.checkBoxClickables.Text = "Ruby farming";
            this.checkBoxClickables.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(119, 178);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(240, 243);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // labelTextHeroAlign
            // 
            this.labelTextHeroAlign.Location = new System.Drawing.Point(13, 272);
            this.labelTextHeroAlign.Name = "labelTextHeroAlign";
            this.labelTextHeroAlign.Size = new System.Drawing.Size(100, 83);
            this.labelTextHeroAlign.TabIndex = 4;
            this.labelTextHeroAlign.Text = "For Hero Leveling align your Hero list that the top one\'s border is at the top as" +
    " shown on picture.";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 421);
            this.Controls.Add(this.labelTextHeroAlign);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBoxClickables);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clicker Hero";
            this.groupBox3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxClickables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
