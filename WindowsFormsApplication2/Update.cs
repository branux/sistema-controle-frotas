﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace Sistema_Controle

{
   class Update
    {
        bool yn, avisar;

        public bool Avisar
        {
            get { return avisar; }
            set { avisar = value; }
        }
        public bool Yn
        {
            get { return yn; }
            set { yn = value; }
        }
        public void up()
        {
            
            string donwloadurl = "";
            Version newVersion = null;

            string xmlURL = @"\\\fileserver-03\\SAUDE\\Mapa_de_Leitos\\Sistemas - Vinicius\\Sistema de Controle de Ambulancias\\update.xml";
            XmlTextReader reader = null;

            try
            {
                reader = new XmlTextReader(xmlURL);
                reader.MoveToContent();
                string elemeto = "";

                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "coolapp"))
                {
                    while(reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elemeto = reader.Name;
                        }
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch(elemeto)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        donwloadurl = reader.Value;
                                        break;

                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Sistema não está atualizado, podendo conter erros ao continuar usando a versão antiga."+ ex.Message , Application.ProductName );
            }
            finally
            {
                if(reader != null)
                
                    reader.Close();
            }
            Version appverion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (appverion.CompareTo(newVersion) < 0)
            {
                avisar = true;
                yn = true;
                Process.Start(donwloadurl);
            }
            else
            {
                avisar = false;
                yn = false;
                Console.WriteLine("Atualizado");
            }
        }
    }



    
}
