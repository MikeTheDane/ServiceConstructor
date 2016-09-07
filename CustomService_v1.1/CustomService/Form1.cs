using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace CustomService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string header = "DA1458x Custom GATT Service Constructor (v1.1)";

        public XmlDocument customServicesXml = new XmlDocument();

        public int activeSvc = 0;
        
        struct Services
        {
            public string uuid;
            public string activeCharName;
            public int numChar;

            public Services(string charName)
            {
                uuid  = Guid.NewGuid().ToString();
                activeCharName = charName;
                numChar = 1;
            } 
        }

        Services[] services = new Services[2];

        Services[] servicesTemp = new Services[2];

        struct Characteristic
        {
            public string name;
            public string uuid;
            public bool add_user_description;
            public decimal size_of_char;
            public bool prop_broadcast;
            public bool prop_read;
            public bool prop_write_no_response;
            public bool prop_write;
            public bool prop_notify;
            public bool prop_indicate;
            public bool prop_authenticate;
            public bool prop_extended_char;
            public bool perm_read_only;
            public bool perm_write_only;
            public bool perm_read_and_write;
            public bool authentication_required;
            public bool authorization_required;

            public Characteristic (string char_name)
            {
                name = char_name;
                uuid = Guid.NewGuid().ToString();
                add_user_description = false;
                size_of_char = 1;
                prop_broadcast = false;
                prop_read = true;
                prop_write_no_response = false;
                prop_write = true;
                prop_notify = false;
                prop_indicate = false;
                prop_authenticate = false;
                prop_extended_char = false;
                perm_read_only = false;
                perm_write_only = false;
                perm_read_and_write = true;
                authentication_required = false;
                authorization_required = false;
            }
        }

        List<Characteristic>[] svcArray = new List<Characteristic>[2];

        List<Characteristic>[] svcArrayTemp = new List<Characteristic>[2];
       
        public bool updatingTab = false;
       
        bool hasBeenEdited = false;

        string project_path = "";

        int MAXIMUM_NUMBER_OF_SERVICES = 2;

        public string uuid_text(string uuid)
        {
            string s="";
            string t="";
            uuid = uuid.ToUpper();
            string mask = "0123456789ABCDEF";
            for (int i = 0; i < uuid.Length; i++)
              if (mask.IndexOf(uuid[i]) >= 0) s += uuid[i];
            if (s.Length != 4)
            {
                for (int i = 0; i < s.Length; i += 2)
                {
                    if (i != 0) t += ", ";
                    t += "0x";
                    t += s[s.Length - 2 - i];
                    t += s[s.Length - 1 - i];
                }
                s = "{" + t + "}";
            } 
            return s;
        }

        public string capitalize(string s)
        {
            string t = "";
            s = s.ToUpper();
            for (int i = 0; i<s.Length; i++)
            {
                if (s[i] == ' ')
                    t += "_";
                else
                    t += s[i];
            }
            return t;
        }

        public string lowerCase(string s)
        {
            string t = "";
            s = s.ToLower();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                    t += "_";
                else
                    t += s[i];
            }
            return t;
        }

        public string uncapitalize(string s)
        {
            bool nextIsUpper = false;
            string t = "";
            s = s.ToLower().Replace('_',' ');
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                    nextIsUpper = true;
                else
                {
                    if (nextIsUpper)
                    {
                        nextIsUpper = false;
                        t += s[i].ToString().ToUpper();
                    }
                    else
                    {
                        t += s[i];
                    }
                }
            }
            return t;
        }

        private void randomSvcUuidButton_Click(object sender, EventArgs e)
        {
            var guid = Guid.NewGuid().ToString();
            textBox1.Text = guid;
            hasBeenEdited = true;
            this.Text = header + "*";
        }

        private void addCharButton_Click(object sender, EventArgs e)
        {

            storeTabData();
            services[activeSvc].numChar++;
            string newCharName = "CharS" + (activeSvc+1).ToString() + "C" + services[activeSvc].numChar;
            Characteristic tempChar = new Characteristic(newCharName);
            svcArray[activeSvc].Add(tempChar);
            services[activeSvc].activeCharName = newCharName;

            TabPage new_page = new TabPage();
            new_page.BackColor = Color.White;
            updatingTab = true;
            tabControl1.TabPages.Add(new_page);
            tabControl1.SelectedIndex = tabControl1.TabCount - 1;

            displayData();

            textBox2.Focus();
            textBox2.SelectAll();
            hasBeenEdited = true;
            this.Text = header + "*";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!updatingTab)
            {
                Characteristic tempChar = new Characteristic();
                tempChar = svcArray[activeSvc].Find(x => x.name == tabControl1.SelectedTab.Text);
                int i = svcArray[activeSvc].FindIndex(x => x.name == tabControl1.SelectedTab.Text);
                tabControl1.SelectedTab.Text = textBox2.Text;
                tempChar.name = textBox2.Text;
                svcArray[activeSvc][i] = tempChar;
                services[activeSvc].activeCharName = textBox2.Text;
            }
        }

        private void deleteCharButton_Click(object sender, EventArgs e)
        {
            string tempStr = "Are you sure that you want to delete\n" + tabControl1.SelectedTab.Text + "?";
            DialogResult dialogResult = MessageBox.Show(tempStr, "Deleting Characteristic", MessageBoxButtons.YesNoCancel);
            if (dialogResult == DialogResult.Yes)
            {
                updatingTab = true;
                int i = tabControl1.SelectedIndex - 1;
                if (tabControl1.TabCount > 1)
                    tabControl1.TabPages.Remove(tabControl1.SelectedTab);
                if (tabControl1.TabCount == 1)
                    deleteCharButton.Visible = false;
                else
                    deleteCharButton.Visible = true;
                
                svcArray[activeSvc].Remove(svcArray[activeSvc].Find(x => x.name == services[activeSvc].activeCharName));
                
                if (i < 0)
                    i = 0;
                services[activeSvc].activeCharName = tabControl1.TabPages[i].Text;
                tabControl1.SelectTab(i);
                displayData();
                hasBeenEdited = true;
                this.Text = header + "*";
            }
        }
        public void displayData()
        {
            updatingTab = true;
            Characteristic tempChar = new Characteristic();
            tempChar = svcArray[activeSvc].Find(x => x.name == services[activeSvc].activeCharName);
            tabControl1.SelectedTab.Text = tempChar.name;
            textBox2.Text = tempChar.name;
            checkBox3.Checked = tempChar.add_user_description;
            textBox3.Text = tempChar.uuid;
            numericUpDown1.Value = tempChar.size_of_char;

            checkBox4.Checked = tempChar.prop_read;
            checkBox5.Checked = tempChar.prop_write;
            checkBox6.Checked = tempChar.prop_write_no_response;
            checkBox7.Checked = tempChar.prop_notify;
            checkBox8.Checked = tempChar.prop_extended_char;
            checkBox9.Checked = tempChar.prop_authenticate;
            checkBox10.Checked = tempChar.prop_broadcast;
            checkBox11.Checked = tempChar.prop_indicate;

            checkBox1.Checked = tempChar.authentication_required;
            checkBox2.Checked = tempChar.authorization_required;

            radioButton1.Checked = tempChar.perm_read_only;
            radioButton2.Checked = tempChar.perm_write_only;
            radioButton3.Checked = tempChar.perm_read_and_write;

            updatingTab = false;

            if (svcArray[activeSvc].Count > 1)
                deleteCharButton.Visible = true;
            else
                deleteCharButton.Visible = false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updatingTab)
            {
                services[activeSvc].activeCharName = tabControl1.SelectedTab.Text;
                displayData();
            }
        }
                    
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = header;

            svcArray[0] = new List<Characteristic>();
            svcArray[1] = new List<Characteristic>();

            services[0] = new Services("CharS1C1");
            services[1] = new Services("CharS2C1");

            svcArrayTemp[0] = new List<Characteristic>();
            svcArrayTemp[1] = new List<Characteristic>();

            servicesTemp[0] = new Services();
            servicesTemp[1] = new Services();


            Characteristic tempChar1 = new Characteristic("CharS1C1");
            svcArray[0].Add(tempChar1);
            Characteristic tempChar2 = new Characteristic("CharS2C1");
            svcArray[1].Add(tempChar2);

            tabControl1.SelectedIndex = 0;
            tabControl2.SelectedIndex = 0;
            label6.Text = "Service 1";
            textBox1.Text = services[0].uuid;

            displayData();

            textBox2.Focus();
            textBox2.SelectAll();
        }

        private void randomCharUuidButton_Click(object sender, EventArgs e)
        {
            var guid = Guid.NewGuid().ToString();
            textBox3.Text = guid;
            hasBeenEdited = true;
            this.Text = header + "*";
        }

        private void storeTabData()
        {
            if (!updatingTab)
            {
                Characteristic tempChar = new Characteristic();

                tempChar.name = textBox2.Text;
                tempChar.add_user_description = checkBox3.Checked;
                tempChar.uuid = textBox3.Text;
                tempChar.authentication_required = checkBox1.Checked;
                tempChar.authorization_required = checkBox2.Checked;

                tempChar.perm_read_only = radioButton1.Checked;
                tempChar.perm_read_and_write = radioButton3.Checked;
                tempChar.perm_write_only = radioButton2.Checked;

                tempChar.prop_read = checkBox4.Checked;
                tempChar.prop_write = checkBox5.Checked;
                tempChar.prop_write_no_response = checkBox6.Checked;
                tempChar.prop_notify = checkBox7.Checked;
                tempChar.prop_indicate = checkBox11.Checked;
                tempChar.prop_authenticate = checkBox9.Checked;
                tempChar.prop_extended_char = checkBox8.Checked;
                tempChar.prop_broadcast = checkBox10.Checked;

                tempChar.size_of_char = numericUpDown1.Value;

                int i = svcArray[activeSvc].FindIndex(x => x.name == tempChar.name);
                svcArray[activeSvc][i] = tempChar;

                services[activeSvc].activeCharName = textBox2.Text;
                services[activeSvc].uuid = textBox1.Text;
            }
        }

        private void tabControl1_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl1.SelectedTab != null)
              storeTabData();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value == 1)
                label5.Text = "Octet";
            else
                label5.Text = "Octets";
            if (numericUpDown1.Value > numericUpDown1.Maximum)
                numericUpDown1.Value = numericUpDown1.Maximum;
            if (!updatingTab)
            {
                hasBeenEdited = true;
                this.Text = header + "*";
            }
        }

        private void createXml()
        {

            customServicesXml.RemoveAll();
            XmlNode docNode = customServicesXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            customServicesXml.AppendChild(docNode);

            XmlNode servicesNode = customServicesXml.CreateNode(XmlNodeType.Element, "services", null);

            XmlNode thisNode = customServicesXml.CreateNode(XmlNodeType.Element, "date-time", null);
            XmlNode thisText = customServicesXml.CreateTextNode(DateTime.Now.ToString());
            thisNode.AppendChild(thisText);
            servicesNode.AppendChild(thisNode);

            thisNode = customServicesXml.CreateNode(XmlNodeType.Element, "exclude-service-2", null);
            if(checkBox12.Checked)
                thisText = customServicesXml.CreateTextNode("true");
            else
                thisText = customServicesXml.CreateTextNode("false");
            thisNode.AppendChild(thisText);
            servicesNode.AppendChild(thisNode);

            for (int i = 0; i < 2; i++)
            {
                string serviceName = "service";
                XmlNode serviceNode = customServicesXml.CreateNode(XmlNodeType.Element, serviceName, null);

                thisNode = customServicesXml.CreateNode(XmlNodeType.Element, "service-uuid", null);
                thisText = customServicesXml.CreateTextNode(services[i].uuid);
                thisNode.AppendChild(thisText);
                serviceNode.AppendChild(thisNode);

                thisNode = customServicesXml.CreateNode(XmlNodeType.Element, "num-char", null);
                thisText = customServicesXml.CreateTextNode(services[i].numChar.ToString());
                thisNode.AppendChild(thisText);
                serviceNode.AppendChild(thisNode);

                XmlNode charsNode = customServicesXml.CreateNode(XmlNodeType.Element, "characteristics", null);
                foreach (Characteristic characteristic in svcArray[i])
                {
                    XmlNode charNode = customServicesXml.CreateNode(XmlNodeType.Element, "characteristic", null);

                    XmlNode nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "name", null);
                    XmlNode data = customServicesXml.CreateTextNode(characteristic.name);
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "user-description", null);
                    if (characteristic.add_user_description)
                      data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "uuid", null);
                    data = customServicesXml.CreateTextNode(characteristic.uuid);
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "size", null);
                    data = customServicesXml.CreateTextNode(characteristic.size_of_char.ToString());
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "read", null);
                    if (characteristic.prop_read)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "write", null);
                    if (characteristic.prop_write)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "write-no-response", null);
                    if (characteristic.prop_write_no_response)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "notify", null);
                    if (characteristic.prop_notify)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "indicate", null);
                    if (characteristic.prop_indicate)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "broadcast", null);
                    if (characteristic.prop_broadcast)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "authenticate", null);
                    if (characteristic.prop_authenticate)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "extended-properties", null);
                    if (characteristic.prop_extended_char)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "read-only", null);
                    if (characteristic.perm_read_only)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "write-only", null);
                    if (characteristic.perm_write_only)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "read-and-write", null);
                    if (characteristic.perm_read_and_write)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "authentication-required", null);
                    if (characteristic.authentication_required)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);

                    nameNode = customServicesXml.CreateNode(XmlNodeType.Element, "authorization-required", null);
                    if (characteristic.authorization_required)
                        data = customServicesXml.CreateTextNode("true");
                    else
                        data = customServicesXml.CreateTextNode("false");
                    nameNode.AppendChild(data);
                    charNode.AppendChild(nameNode);
                    
                    charsNode.AppendChild(charNode);
                }

                serviceNode.AppendChild(charsNode);
                servicesNode.AppendChild(serviceNode);
                
            }
            
            customServicesXml.AppendChild(servicesNode);

        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            storeTabData();

            createXml();

            int number_of_services = MAXIMUM_NUMBER_OF_SERVICES;
            if (checkBox12.Checked)
                number_of_services = 1;

            // TBD: Validity check
            richTextBox2.Clear();

            string hfile = richTextBox4.Text + "\n\n";
            hfile = hfile.Replace("***version***", header);
            hfile = hfile.Replace("***Time***", DateTime.Now.ToString());

            //int q = 0;
            for (int q = 0; q < number_of_services; q++)
            {


                string qq = (q + 1).ToString();

                // Add service uuid definition
                string tempStr = uuid_text(services[q].uuid);
                if (tempStr.Length == 4)
                    hfile += "#define DEF_CUST" + qq + "_SVC_UUID_16   0x" + tempStr + "\n\n";
                else
                    hfile += "#define DEF_CUST" + qq + "_SVC_UUID_128   " + tempStr + "\n\n";

                // Char UUIDs:
                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = svcArray[q][i];
                    if (uuid_text(new_char.uuid).Length > 4)
                    {
                        hfile += "#define DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_128  ";
                        hfile += uuid_text(new_char.uuid) + "\n";
                    }
                    else
                    {
                        tempStr = uuid_text(new_char.uuid);
                        tempStr = "{0x" + tempStr.Substring(2, 2) + ", 0x" + tempStr.Substring(0, 2) + "}";
                        hfile += "#define DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_16  ";
                        hfile += tempStr + "\n";
                    }
                }
                hfile += "\n\n";
            }
            for (int q = 0; q < number_of_services; q++)
            {

                string qq = (q + 1).ToString();

                // Length of chars:
                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = svcArray[q][i];
                    hfile += "#define DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_LEN  ";
                    hfile += new_char.size_of_char.ToString() + "\n";
                }

                hfile += "\n";

                // User descriptions of chars:
                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = svcArray[q][i];
                    if (new_char.add_user_description)
                    {
                        hfile += "#define CUST" + qq + "_" + capitalize(new_char.name) + "_USER_DESC  ";
                        hfile += "\"" + new_char.name + "\"\n";
                    }
                    
                }
                hfile += "\n";
            }

            hfile += "\n///Attribute Database Index Enumeration\nenum\n{\n";

            for (int q = 0; q < number_of_services; q++)
            {

                string qq = (q + 1).ToString();
                
                // Enum of attributes:
                if(q==0)
                    hfile += "\tCUST" + qq + "_IDX_SVC = 0,\n\n";
                else
                    hfile += "\tCUST" + qq + "_IDX_SVC,\n\n";

                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = svcArray[q][i];
                    hfile += "\tCUST" + qq + "_IDX_" + capitalize(new_char.name) + "_CHAR,\n";
                    hfile += "\tCUST" + qq + "_IDX_" + capitalize(new_char.name) + "_VAL,\n";
                    if (new_char.prop_indicate || new_char.prop_notify)
                        hfile += "\tCUST" + qq + "_IDX_" + capitalize(new_char.name) + "_CFG,\n";
                    if (new_char.add_user_description)
                        hfile += "\tCUST" + qq + "_IDX_" + capitalize(new_char.name) + "_USER_DESC,\n";
                    hfile += "\n";
                    
                }
                
            }
            hfile += "\tCUST1_IDX_NB\n};\n\n" + richTextBox3.Text + "\n\n";

            
            string cfile = richTextBox7.Text + "\n";
            cfile = cfile.Replace("***version***", header);
            cfile = cfile.Replace("***Time***", DateTime.Now.ToString());

            for (int q = 0; q < number_of_services; q++)
            {
                string qq = (q + 1).ToString();
                // attribute values
                string tempStr = uuid_text(services[q].uuid);
                if (tempStr.Length == 4)
                    cfile += "static const att_svc_desc_t custs" + qq + "_svc                       = DEF_CUST" + qq + "_SVC_UUID_16;\n\n";
                else
                    cfile += "static const att_svc_desc128_t custs" + qq + "_svc                        = DEF_CUST" + qq + "_SVC_UUID_128;\n\n";

                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = svcArray[q][i];
                    if (uuid_text(new_char.uuid).Length > 4)
                    {
                        cfile += "static uint8_t CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_128[ATT_UUID_128_LEN]     ";
                        cfile += "\t= DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_128;\n";
                    }
                    else
                    {
                        cfile += "static uint8_t CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_16[ATT_UUID_16_LEN]     ";
                        cfile += "\t= DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_16;\n";
                    }
                }
                cfile += "\n";
            }

            for (int q = 0; q < number_of_services; q++)
            {
                string qq = (q + 1).ToString();
                // Characteristic descriptors
                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = svcArray[q][i];
                    string tempcfile = "";
                    if (uuid_text(new_char.uuid).Length > 4)
                        cfile += "static const struct att_char128_desc custs" + qq + "_" + lowerCase(new_char.name) + "_char =\n{\n\t";
                    else
                        cfile += "static const struct att_char_desc custs" + qq + "_" + lowerCase(new_char.name) + "_char =\n{\n\t";
                    if (new_char.prop_read)
                        tempcfile += "ATT_CHAR_PROP_RD | ";
                    if (new_char.prop_write)
                        tempcfile += "ATT_CHAR_PROP_WR | ";
                    if (new_char.prop_write_no_response)
                        tempcfile += "ATT_CHAR_PROP_WR_NO_RESP | ";
                    if (new_char.prop_notify)
                        tempcfile += "ATT_CHAR_PROP_NTF | ";
                    if (new_char.prop_indicate)
                        tempcfile += "ATT_CHAR_PROP_IND | ";

                    if (new_char.prop_authenticate)
                        tempcfile += "ATT_CHAR_PROP_AUTH | ";
                    if (new_char.prop_extended_char)
                        tempcfile += "ATT_CHAR_PROP_EXT_PROP | ";
                    if (new_char.prop_broadcast)
                        tempcfile += "ATT_CHAR_PROP_BCAST | ";
                    if (tempcfile == "")
                        cfile += "0 **";
                    else
                        cfile += tempcfile;
                    // Backtrack 3 characters
                    cfile = cfile.Substring(0, cfile.Length - 3) + ",\n\t{0, 0},\n\t";
                    if (uuid_text(new_char.uuid).Length > 4)
                        cfile += "DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_128";
                    else
                        cfile += "DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_16";
                    cfile += "\n};\n\n";
                }
            }
            cfile += richTextBox5.Text + "\n\n";

            // Remove declarations that are not required
            int client_configuration_count = 0;
            int user_description_count = 0;
            for (int q = 0; q < number_of_services; q++)
            {
                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    if (new_char.prop_indicate || new_char.prop_notify)
                        client_configuration_count++;
                    if (new_char.add_user_description)
                        user_description_count++;
                }
            }
            if (client_configuration_count == 0)
                cfile = cfile.Replace("static const uint16_t att_decl_cfg       = ATT_DESC_CLIENT_CHAR_CFG;\n", "");
            if (user_description_count == 0)
                cfile = cfile.Replace("static const uint16_t att_decl_user_desc = ATT_DESC_CHAR_USER_DESCRIPTION;\n", "");
           

            for (int q = 0; q < number_of_services; q++)
            {
                string qq = (q + 1).ToString();

                cfile += "\t// -----------  Attributes of Custom Service #" + qq + " -----------//\n";

                cfile += "\t[CUST" + qq + "_IDX_SVC] =\n\t{\n\t\t(uint8_t*)&att_decl_svc,";


                cfile += "\n\t\tATT_UUID_16_LEN,\n\t\tPERM(RD, ENABLE),\n\t\tsizeof(custs" + qq + "_svc),\n\t\t";
                cfile += "sizeof(custs" + qq + "_svc),\n\t\t(uint8_t*)&custs" + qq + "_svc\n\t},\n\n";


                for (int i = 0; i < services[q].numChar; i++)
                {
                    Characteristic new_char = new Characteristic();
                    new_char = new_char = svcArray[q][i];

                    // Characteristic Declaration
                    cfile += "\t// " + new_char.name + " Characteristic Attribute\n";
                    cfile += "\t[CUST" + qq + "_IDX_" + capitalize(new_char.name) + "_CHAR] =\n\t{\n\t\t";
                    cfile += "(uint8_t*)&att_decl_char,\n\t\tATT_UUID_16_LEN,\n\t\t";
                    cfile += "PERM(RD, ENABLE),\n\t\t";
                    string s = "custs" + qq + "_" + lowerCase(new_char.name) + "_char";
                    cfile += "sizeof(" + s + "),\n\t\tsizeof(" + s + "),\n\t\t(uint8_t*)&" + s + "\n\t},\n\n";


                    // Characteristic Value Declaration
                    cfile += "\t// " + new_char.name + " Characteristic Value Attribute\n";
                    cfile += "\t[CUST" + qq + "_IDX_" + capitalize(new_char.name) + "_VAL] =\n\t{\n\t\t";


                    if (uuid_text(new_char.uuid).Length > 4)
                        cfile += "CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_128,\n\t\tATT_UUID_128_LEN,\n\t\t";
                    else
                        cfile += "CUST" + qq + "_" + capitalize(new_char.name) + "_UUID_16,\n\t\tATT_UUID_16_LEN,\n\t\t";
                    string access = "ENABLE";
                    if (new_char.authentication_required)
                        access = "AUTH";
                    if (new_char.authorization_required)
                        access = "AUTHZ";

                    if (new_char.perm_read_only)
                        cfile += "PERM(RD," + access + "),\n\t\t";
                    if (new_char.perm_write_only)
                        cfile += "PERM(WR," + access + "),\n\t\t";
                    if (new_char.perm_read_and_write)
                        cfile += "PERM(WR," + access + ") | PERM(RD," + access + "),\n\t\t";
                    cfile += "DEF_CUST" + qq + "_" + capitalize(new_char.name) + "_LEN,\n\t\t0,\n\t\tNULL\n\t},\n\n";

                    // Characteristic Client Configuration
                    if (new_char.prop_notify || new_char.prop_indicate)
                    {
                        cfile += "\t// " + new_char.name + " Characteristic Client Configuration Attribute\n";
                        cfile += "\t[CUST" + qq + "_IDX_" + capitalize(new_char.name) + "_CFG] =\n\t{\n\t\t";
                        cfile += "(uint8_t*)&att_decl_cfg,\n\t\tATT_UUID_16_LEN,\n\t\t";
                        cfile += "PERM(RD, " + access + ") | PERM(WR, " + access + "),\n\t\t";
                        cfile += "sizeof(uint16_t),\n\t\t0,\n\t\tNULL\n\t},\n\n";
                    }

                    // Characteristic User Description
                    if (new_char.add_user_description)
                    {
                        s = "CUST" + qq + "_" + capitalize(new_char.name) + "_USER_DESC";
                        cfile += "\t// " + new_char.name + " Characteristic User Description Attribute\n";
                        cfile += "\t[CUST" + qq + "_IDX_" + capitalize(new_char.name) + "_USER_DESC] =\n\t{\n\t\t";
                        cfile += "(uint8_t*)&att_decl_user_desc,\n\t\tATT_UUID_16_LEN,\n\t\tPERM(RD, ENABLE),\n\t\t";
                        cfile += "sizeof(" + s + ") - 1,\n\t\tsizeof(" + s + ") - 1,\n\t\t" + s + "\n\t},\n\n";
                    }
                }

            }
            cfile += "};\n\n";
            cfile += "/// @} USER_CONFIG\n\r";
            

            folderBrowserDialog1.SelectedPath = project_path;
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string tStr = "Overwrite existing project files?";
                DialogResult confirm = MessageBox.Show(tStr, "Saving project files", MessageBoxButtons.YesNoCancel);
                if (confirm != DialogResult.Yes)
                    return;

                System.IO.File.WriteAllText(folderBrowserDialog1.SelectedPath + "/user_custs1_def.h", hfile);
                System.IO.File.WriteAllText(folderBrowserDialog1.SelectedPath + "/user_custs1_def.c", cfile);

            }
        }

        
        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
             
        }

        private void tabControl2_Selected(object sender, TabControlEventArgs e)
        {
            storeTabData();
            services[activeSvc].uuid = textBox1.Text;
            updatingTab = true;
            label6.Text = tabControl2.SelectedTab.Text;
            tabControl1.TabPages.Clear();
            activeSvc = tabControl2.SelectedIndex;

            textBox1.Text = services[activeSvc].uuid;
            foreach (Characteristic c in svcArray[activeSvc])
            {
                TabPage t = new TabPage();
                t.Text = c.name;
                tabControl1.TabPages.Add(t);
                if (c.name == services[activeSvc].activeCharName)
                {
                    
                    tabControl1.SelectedTab = t;
                    tabControl1.SelectedTab.Text = services[activeSvc].activeCharName;
                    displayData();
                }
            }
       }

        private void fileOpenButton_Click(object sender, EventArgs e)
        {
            if (hasBeenEdited)
            {
                string tempStr = "Discard changes?";
                DialogResult confirm = MessageBox.Show(tempStr, "Opening XML file", MessageBoxButtons.YesNoCancel);
                if (confirm != DialogResult.Yes)
                    return;
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                project_path = (string)(new System.IO.FileInfo(openFileDialog1.FileName).DirectoryName);
                
                bool xmlIsValid = false;

                svcArrayTemp[0].Clear();
                svcArrayTemp[1].Clear();
                
                int svcIdx = -1;

                XmlTextReader reader = new XmlTextReader(openFileDialog1.FileName);

                Characteristic c = new Characteristic();

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "services":
                                xmlIsValid = true;
                                break;

                            case "exclude-service-2":
                                reader.Read();
                                if (reader.Value == "true")
                                    checkBox12.Checked = true;
                                else
                                    checkBox12.Checked = false;
                                break;
                            case "service":
                                svcIdx++;
                                break;
                            case "service-uuid":
                                reader.Read();
                                servicesTemp[svcIdx].uuid = reader.Value;
                                break;
                               
                            case "num-char":
                                reader.Read();
                                servicesTemp[svcIdx].numChar = Int32.Parse(reader.Value);
                                break;

                            case "name":
                                reader.Read();
                                c.name = reader.Value;
                                break;

                            case "user-description":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.add_user_description = true;
                                else
                                    c.add_user_description = false;
                                break;

                            case "uuid":
                                reader.Read();
                                c.uuid = reader.Value;
                                break;

                            case "size":
                                reader.Read();
                                c.size_of_char = Int32.Parse(reader.Value);
                                break;


                            case "read":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_read = true;
                                else
                                    c.prop_read = false;
                                break;

                            case "write":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_write = true;
                                else
                                    c.prop_write = false;
                                break;

                            case "write-no-response":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_write_no_response = true;
                                else
                                    c.prop_write_no_response = false;
                                break;

                            case "notify":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_notify = true;
                                else
                                    c.prop_notify = false;
                                break;

                            case "indicate":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_indicate = true;
                                else
                                    c.prop_indicate = false;
                                break;

                            case "broadcast":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_broadcast = true;
                                else
                                    c.prop_broadcast = false;
                                break;

                            case "authenticate":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_authenticate = true;
                                else
                                    c.prop_authenticate = false;
                                break;

                            case "extended-properties":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.prop_extended_char = true;
                                else
                                    c.prop_extended_char = false;
                                break;

                            case "read-only":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.perm_read_only = true;
                                else
                                    c.perm_read_only = false;
                                break;

                            case "write-only":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.perm_write_only = true;
                                else
                                    c.perm_write_only = false;
                                break;

                            case "read-and-write":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.perm_read_and_write = true;
                                else
                                    c.perm_read_and_write = false;
                                break;

                            case "authentication-required":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.authentication_required = true;
                                else
                                    c.authentication_required = false;
                                break;

                            case "authorization-required":
                                reader.Read();
                                if (reader.Value == "true")
                                    c.authorization_required = true;
                                else
                                    c.authorization_required = false;
                                svcArrayTemp[svcIdx].Add(c);
                                break;

                        }
                    }
                    

                }

                reader.Close();

                if (xmlIsValid)
                {
                    this.Text = header;
                    for (int i = 0; i < 2; i++)
                    {
                        svcArray[i].Clear();
                        foreach(Characteristic ch in svcArrayTemp[i])
                        {
                            svcArray[i].Add(ch);
                        }
                        services[i].numChar = servicesTemp[i].numChar;
                        services[i].uuid = servicesTemp[i].uuid;
                        services[i].activeCharName = servicesTemp[i].activeCharName;
                    }
                    
                    updatingTab = true;
                    tabControl1.TabPages.Clear();
                    foreach (Characteristic tempChar in svcArray[0])
                    {
                        TabPage new_page = new TabPage();
                        new_page.BackColor = Color.White;
                        new_page.Text = tempChar.name;
                        updatingTab = true;
                        tabControl1.TabPages.Add(new_page);
                    }

                    tabControl1.SelectedIndex = 0;
                    tabControl2.SelectedIndex = 0;

                    services[0].activeCharName = svcArray[0].First().name;
                    services[1].activeCharName = svcArray[1].First().name;

                    activeSvc = 0;

                    label6.Text = tabControl2.SelectedTab.Text;
                    textBox1.Text = services[activeSvc].uuid;
                    
                }
                else
                {
                    string errorStr = "Invalid XML format!";
                    DialogResult dialogResult = MessageBox.Show(errorStr, "Opening XML file", MessageBoxButtons.OK);
                }
                displayData();
            }
        }

        private void fileSaveButton_Click(object sender, EventArgs e)
        {
            storeTabData();
            
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog2.FileName != "")
                {
                    project_path = (string)(new System.IO.FileInfo(saveFileDialog2.FileName).DirectoryName);
                    this.Text = header;
                    hasBeenEdited = false;
                    createXml();
                    customServicesXml.Save(saveFileDialog2.FileName);
                }
            }
            

        }

        private void editsWereMade(object sender, KeyPressEventArgs e)
        {
            hasBeenEdited = true;
            this.Text = header+"*";
            services[activeSvc].activeCharName = textBox2.Text;
            services[activeSvc].uuid = textBox1.Text;
            storeTabData();

        }

        private void randomSvcUuidButton_Click_1(object sender, EventArgs e)
        {
            hasBeenEdited = true;
            this.Text = header + "*";
        }

        private void anyCheckBoxClicked(object sender, EventArgs e)
        {
            hasBeenEdited = true;
            this.Text = header + "*";
        }

        private void richTextBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (hasBeenEdited)
            {
                // Display a MsgBox asking the user to save changes or abort.
                if (MessageBox.Show("Close Without Saving?", "Custom Service Constructor",
                   MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    // Cancel the Closing event from closing the form.
                    e.Cancel = true;
                    // Call method to save file...
                    storeTabData();

                    if (saveFileDialog2.ShowDialog() == DialogResult.OK)
                    {
                        if (saveFileDialog2.FileName != "")
                        {
                            project_path = (string)(new System.IO.FileInfo(saveFileDialog2.FileName).DirectoryName);
                            this.Text = header;
                            hasBeenEdited = false;
                            createXml();
                            customServicesXml.Save(saveFileDialog2.FileName);
                        }
                    }
                }
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            services[activeSvc].uuid = textBox1.Text;
            storeTabData();
        }
    }
}
