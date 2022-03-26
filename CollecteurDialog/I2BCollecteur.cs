using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;
using XMLSerializer;
using XMLSerializer.SerializeException;

namespace CollecteurDialog
{
    public partial class I2BCollecteur : Form
    {
        private bool collecteurLoaded = false;
        private String collecConfigPath;
        private Config config;
        public I2BCollecteur()
        {

            InitializeComponent();
            checkCollecteurIsRunning();
            initData();
          
        }
        private void checkCollecteurIsRunning()
        {
            Process[] pname = Process.GetProcessesByName("BaliseListner");
            if (pname.Length > 0)
            {
                collecteur = pname[0];
                collecteurLoaded = true;
            }
            for(int i=1;i<pname.Length;i++){
                pname[i].Kill();
            }

        }
        private void initData(){


            collecteur.EnableRaisingEvents = true;
            collecteur.Exited += new EventHandler(collecteurExited);

            if (Properties.Settings.Default.collecFolder.Length == 0)

                this.collecteurPath.Text = Application.StartupPath + "\\bin";
            else
                this.collecteurPath.Text = Properties.Settings.Default.collecFolder;
            config  = new Config("",30002,false);
            collecConfigPath =Application.StartupPath+ @"\config.xml";
            
            try
            {
                config = Utils.loadXMLtoObject<Config>(collecConfigPath);
                   
                
            }
            catch(Exception e){
                if (consoleOut.Text != "")
                    consoleOut.AppendText("\r\n");
                consoleOut.AppendText("Erreur dans l'ouverture de fichier : " + collecConfigPath);
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText(e.Message);
            }
            if (config.IPAddressString.Length > 7)
                ipAddressControl1.Text = config.IPAddressString;
            if (config.Port > 0)
                textBox3.Text = config.Port+"";

            checkBox1.Checked = config.Debug;

            this.folderBrowserDialog1.SelectedPath = this.collecteurPath.Text;
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip1;
             this.redémarerCollecteurToolStripMenuItem.Enabled = collecteurLoaded;
             this.panel2.Enabled = collecteurLoaded;
             if (collecteurLoaded)
             {
                 this.demarerToolStripMenuItem.Text = "Arrêter Collecteur";
                 this.Start.Text = "Arrêter";
             }
             onCollecteurChangeState(collecteurLoaded);

        }

        private void collecteurExited(object sender, EventArgs e)
        {
            if (!collecteurLoaded)
                return;
            onCollecteurChangeState(false);
            if (consoleOut.Text != "")
                consoleOut.AppendText("\r\n");
            consoleOut.AppendText("le Collecteur est arrêté autrement Code : " + collecteur.ExitCode);
            consoleOut.AppendText("\r\n");
            
        }
       

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

     

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result =this.folderBrowserDialog1.ShowDialog();
           
            if (result == DialogResult.OK) // Test result.
            {
                this.collecteurPath.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.collecFolder = this.collecteurPath.Text ;
                Properties.Settings.Default.Save();
            }
        }

        private void I2BCollecteur_Load(object sender, EventArgs e)
        {
           // this.button1
        }

        private void startCollector(){

            if (consoleOut.Text != "")
                consoleOut.AppendText("\r\n");
            config.IPAddressString = ipAddressControl1.Text;
            int ff;
            bool result = Int32.TryParse(textBox3.Text, out ff );
            config.Port = ff;
            config.Debug = checkBox1.Checked;
            consoleOut.AppendText("Démarrage de Collecteur ...");
            collecteur.StartInfo.FileName = collecteurPath.Text + "\\BaliseListner.exe";
            collecteur.StartInfo.CreateNoWindow = false;
            if (!config.Debug)
                 collecteur.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; 
            else
                collecteur.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            try
            {
                config.saveXML("config.xml");
            }
            catch (SerializationXmlConfigExeception sxce)
            {
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText("l'enregistrement de fichier du configuration a echuié ,le collecteur démarre avec ces paramettre par defaut");
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText(sxce.Message);
            }
            try
            {
                collecteur.Start();
              
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText("Démarrage de Collecteur avec succés");
                onCollecteurChangeState(true);
            }
            catch (Exception e)
            {
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText("Démarrage de Collecteur a échoué");
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText ( e.Message);
            }
           
        }
        private void stopCollector()
        {
           
            if (consoleOut.Text != "")
                consoleOut.AppendText("\r\n");
            consoleOut.AppendText("Arrêt  de Collecteur ...");
           
            try
            {

                collecteur.Kill();
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText("Arrêt  de Collecteur  avec succés");
                onCollecteurChangeState(false);


            }
            catch (Exception e)
            {
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText("Arrêt de Collecteur a échoué");
                consoleOut.AppendText("\r\n");
                consoleOut.AppendText(e.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.collecteurPath.Text + @"\Log");
            }
            catch
            {
                MessageBox.Show("le répertoire des logs n'existe pas");
            }
            //cf.EnregistrerConfig(this.collecteurPath.Text + "\\config.xml");
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (collecteurLoaded)
                stopCollector();
            else 
                startCollector();
        }

        private void onCollecteurChangeState(bool start)
        {

            this.panel2.Enabled = !start;
            this.browse.Enabled= !start;
            if (start)
            {
                this.demarerToolStripMenuItem.Text = "Arrêter Collecteur";
                this.Start.Text = "Arrêter";
            }
            else
            {
                this.demarerToolStripMenuItem.Text = "Démarrer Collecteur";
                this.Start.Text = "Démarrer";
            }

            this.redémarerCollecteurToolStripMenuItem.Enabled = start;
            collecteurLoaded = start;

        }

        private void notifyIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
        }

        private void administrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
        }

        private void demarerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (collecteurLoaded)
                stopCollector();
            else
                startCollector();
        }

        private void redémarerCollecteurToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
                stopCollector();
                startCollector();
        }

    }
}
