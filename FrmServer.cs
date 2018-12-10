using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace UDP_Server
{
    public partial class FrmServer : Form
    {
        //Définition d'une classe interne privée pour faciliter l'échange de
        //données entre les threads.
        private class CommunicationData
        {
            public IPEndPoint Client { get; private set; }

            public byte[] Data { get; private set; }

            public CommunicationData(IPEndPoint client, byte[] data)
            {
                Client = client;
                Data = data;
            }
        }

        //Déclaration des objets nécessaires au serveur.
        private UdpClient _broadcaster;

        private bool _continuer = true;
        private Thread _thEcouteur;

        /// <summary>
        /// Constructeur du formulaire Serveur.
        /// </summary>
        public FrmServer()
        {
            InitializeComponent();

            //Initialisation et préparation des objets nécessaires au serveur.
            _broadcaster = new UdpClient();
            _broadcaster.EnableBroadcast = true;
            _broadcaster.Connect(new IPEndPoint(IPAddress.Broadcast, 5053));
            
        }

        /// <summary>
        /// Méthode Démarrer qui sera appelée lorsqu'on aura besoin de démarrer le serveur.
        /// </summary>
        private void Demarrer()
        {
            //Ajoute des informations dans le log et modification des bouttons.
            /*BtnDemarrer.Enabled = false;
            BtnArreter.Enabled = true;*/
            button1.Enabled = false;
            button2.Enabled = true;
            AjouterLog("Démarrage du serveur...");

            //Démarrage du thread d'écoute.
            _continuer = true;
            _thEcouteur = new Thread(new ThreadStart(EcouterReseau));
            _thEcouteur.Start();
        }

        /// <summary>
        /// Méthode Arrêter qui sera appelée lorsqu'on aura besoin d'arrêter le serveur.
        /// </summary>
        /// <param name="attendre">Définie si on attend que les threads aient terminé pour
        /// continuer l'exécution du thread principal. True quand le programme se ferme.</param>
        private void Arreter(bool attendre)
        {
            /*BtnDemarrer.Enabled = true;
            BtnArreter.Enabled = false;*/
            button1.Enabled = true;
            button2.Enabled = false; 
            AjouterLog("Arrêt du serveur...");

            _continuer = false;

            //On attend le thread d'écoute seulement si on le demande et si ce dernier était réellement en train de fonctionner.
            if (attendre && _thEcouteur != null && _thEcouteur.ThreadState == ThreadState.Running)
                _thEcouteur.Join();
        }

        /// <summary>
        /// Méthode qui écoute le réseau en permance en quête d'un message UDP sur le port 1523.
        /// </summary>
        private void EcouterReseau()
        {
            //Création d'un Socket qui servira de serveur de manière sécurisée.
            UdpClient serveur = null;
            bool erreur = false;
            int attempts = 0;

            //J'essaie 3 fois car je veux éviter un plantage au serveur juste pour une question de millisecondes.
            do
            {
                try
                {
                    serveur = new UdpClient(1523);
                }
                catch
                {
                    erreur = true;
                    attempts++;
                    Thread.Sleep(400);
                }
            } while (erreur && attempts < 4);

            //Si c'est vraiment impossible de se lier, on en informe le serveur et on quitte le thread.
            if (serveur == null)
            {
                this.Invoke(new Action<string>(AjouterLog), "Il est impossible de se lier au port 1523. Vérifiez votre configuration réseau.");
                this.Invoke(new Action<bool>(Arreter), false);
                return;
            }

            serveur.Client.ReceiveTimeout = 1000;

            //Boucle infinie d'écoute du réseau.
            while (_continuer)
            {
                try
                {
                    IPEndPoint ip = null;
                    byte[] data = serveur.Receive(ref ip);

                    //Préparation des données à l'aide de la classe interne.
                    CommunicationData cd = new CommunicationData(ip, data);
                    //On lance un nouveau thread avec les données en paramètre.
                    new Thread(new ParameterizedThreadStart(TraiterMessage)).Start(cd);
                }
                catch
                {
                }
            }

            serveur.Close();
        }

        /// <summary>
        /// Méthode en charge de traiter un message entrant.
        /// </summary>
        /// <param name="messageArgs"></param>
        private void TraiterMessage(object messageArgs)
        {
            try
            {
                //On récupère les données entrantes et on les formatte comme il faut.
                CommunicationData data = messageArgs as CommunicationData;
                string message = string.Format("{0}:{1} > {2}", data.Client.Address.ToString(), data.Client.Port, Encoding.Default.GetString(data.Data));

                //On renvoie le message formatté à travers le réseau.
                byte[] donnees = Encoding.Default.GetBytes(message);
                _broadcaster.Send(donnees, donnees.Length);
                this.Invoke(new Action<string>(AjouterLog), message);
            }
            catch { }
        }

        /// <summary>
        /// Méthode en charge d'ajouter au Log les messages.
        /// </summary>
        /// <param name="message"></param>
        private void AjouterLog(string message)
        {
            txtLog.AppendText("\r\n" + DateTime.Now.ToUniversalTime() + " => " + message);
        }

        /// <summary>
        /// Gestion du bouton Arrêt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*
        private void BtnArreter_Click(object sender, EventArgs e)
        {
            Arreter(false);
        }

        /// <summary>
        /// Gestion du bouton Démarrer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDemarrer_Click(object sender, EventArgs e)
        {
            Demarrer();
        }
        */

        /// <summary>
        /// Gestion de la fermeture du formulaire.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Arreter(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Demarrer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Arreter(false);
        }
    }

}
