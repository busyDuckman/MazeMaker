/*  ---------------------------------------------------------------------------------------------------------------------------------------
 *  (C) 2019, Dr Warren Creemers.
 *  This file is subject to the terms and conditions defined in the included file 'LICENSE.txt'
 *  ---------------------------------------------------------------------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WDLibApplicationFramework.ViewAndEdit2D;
using WD_toolbox.Rendering;
using WD_toolbox;
using WD_toolbox.AplicationFramework;
using System.Drawing.Printing;
using Busyducks;

namespace Maze2._0
{
    public partial class FrmMain : Form
    {
        Maze maze;
        Path currentPath;
        bool[,] currentMazeMask;

        String status
        {
            set
            {
                MethodInvoker action = delegate { txtStatus.Text = value; };
                this.BeginInvoke(action);
            }
        }

        bool SideBarVisable { get { return pnlLeft.Visible; } set { sidebarToolStripMenuItem.Checked = value; pnlLeft.Visible = value; } }

        public FrmMain()
        {
            InitializeComponent();
        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {
                vbMain.View = new RasterView2D();
                vbMain.BackColor = Color.LightGray;
                newMaze();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void generateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMaze();
        }

        void newMaze()
        {
            if (maze == null)
            {
                maze = new Maze(32, 32);
                maze.PropertyChanged += maze_PropertyChanged;
                pgMain.SelectedObject = maze;
                this.SetupStatusRespond(txtStatus, maze);
                maze.GenerateWithNewSeed();
                currentPath = null;
                currentMazeMask = null;
                redrawMaze();
            }
            else
            {
                maze.GenerateWithNewSeed();
                currentPath = null;
                currentMazeMask = null;
                redrawMaze();
            }
            txtStatus.Text = "Ready.";
        }

        void maze_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            pgMain.Refresh();
            redrawMaze();
        }

        private void redrawMaze()
        {

            if (maze != null)
            {
                //r.FillRectangle(Color.White, bounds);
                Bitmap b = renderMazeAsImage(true);

                ((RasterView2D)vbMain.View).Image = b;

                MethodInvoker action = delegate { vbMain.Refresh(); };
                this.BeginInvoke(action);
            }
        }

        private Bitmap renderMazeAsImage(bool systemDPI = false)
        {
            if(maze == null) {
                return null;
            }
            int gap = 32;
            Rectangle renderBounds = new Rectangle(gap, gap, maze.TotalSizeInPixels.Width, maze.TotalSizeInPixels.Height);
            IRenderer r = IRendererFactory.GetPreferredRenderer(renderBounds.Width + (gap * 2), renderBounds.Height + (gap * 2));
            r.Clear(maze.FillColor);
            maze.Render(r, renderBounds);

            if (currentPath != null)
            {
                maze.renderPath(r, renderBounds, currentPath);
            }
            if (currentMazeMask != null)
            {
                maze.renderMask(r, renderBounds, currentMazeMask);
            }

            

            Bitmap b = r.RenderTargetAsGDIBitmap();
            if (!systemDPI)
            {
                b.SetResolution((float)maze.DPI, (float)maze.DPI);
            }
            return b;
        }

        private void optimiseStartFinishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (maze != null)
            {
                Path p = maze.findLongestStartAndFinish();
                this.updatePath(p);

                /*WD_toolbox.Threading.ThreadHelpers.RunAsBackground(
                    delegate()
                    {
                        Path p = maze.findLongestStartAndFinish();
                        this.updatePath(p);
                    });*/
            }
        }

        public void updatePath(Path p)
        {
            if (p == null)
            {
                currentPath = null;
                return;
            }

            if (p.Count > 0)
            {
                maze[p[0]].Type = Maze.NodeType.Start;
                maze[p.CurrentPos].Type = Maze.NodeType.End;
                currentPath = p;
                redrawMaze();
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //WD_toolbox.AplicationFramework.
        }

        private void vbMain_Load(object sender, EventArgs e)
        {

        }

        private void sidebarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SideBarVisable = !SideBarVisable;
        }

        private void findDeadEndsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(maze != null)
            {
                currentMazeMask = maze.findDeadEndPassages();
                redrawMaze();
            }
        }

        private void generateToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBoxBusyDucks about = new AboutBoxBusyDucks("https://github.com/busyDuckman/MazeMaker", "Copyright (C) 2019, Dr Warren Creemers.", ProductStatus.Beta);
            about.Acknowledge("Jonathan Peli de Halleux ", "QuickGraph", "https://github.com/YaccConstructor/QuickGraph");
            about.ShowDialog();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(maze != null)
            {
                Clipboard.SetImage(renderMazeAsImage());
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newMaze();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintImage(false);
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintImage(true);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveImage();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveImage();
        }

        private void saveImage()
        {
            if (maze != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.OverwritePrompt = true;
                sfd.FileName = string.Format("maze {0}.png", maze.Description);
                sfd.Title = "Save maze as image";
                sfd.Filter = FileFormat.MakeFileDialogeString(FileFormat.Common_Images, true, "All Image Formats");
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Bitmap b = renderMazeAsImage();
                        b.Save(sfd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://www.busyducks.com");
        }


        
        private void PrintImage(bool preview)
        {
            if (maze != null)
            {
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += pd_PrintPage;

                PrintPreviewDialog ppd = new PrintPreviewDialog();
                ppd.Name = "Print Maze";
                ppd.UseAntiAlias = true;  //better fonts
                ppd.Document = pd;

                if (preview)
                {
                    ppd.ShowDialog();
                }
                else
                {
                    PrintDialog printDialge = new PrintDialog();
                    printDialge.Document = pd;
                    if (printDialge.ShowDialog() == DialogResult.OK)
                    {
                        pd.Print();
                    }
                }

            }
        }

        void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(renderMazeAsImage(), 0, 0);
        }

        private void clearOverlayMarkingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPath = null;
            currentMazeMask = null;
            redrawMaze();
        }

        private void solveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Path> solutions = maze.solve();
            if (solutions.Count > 0)
            {
                currentPath = solutions.First();
            }
            else
            {
                txtStatus.Text = "No solutions";
            }
        }

 
    }
}
