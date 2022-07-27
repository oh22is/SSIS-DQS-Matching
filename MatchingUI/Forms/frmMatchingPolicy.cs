using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;
using Microsoft.SqlServer.MessageBox;
using Microsoft.Ssdqs.Component.Common.Logic;
using Microsoft.Ssdqs.Core.Service.KnowledgebaseManagement.Define;
using Microsoft.Ssdqs.Flow.Notification;
using Microsoft.Ssdqs.Infra.Codes;
using Microsoft.Ssdqs.Matching.Define;
using Microsoft.Ssdqs.Proxy.EntryPoint;
using oh22is.SqlServer.DQS.Properties;
// ReSharper disable CheckNamespace

#pragma warning disable 1587

namespace oh22is.SqlServer.DQS
{
    public partial class FrmMatchingUi : Form
    {

        #region Variables

        /// <summary>
        /// Cache the component metadata, service provider interface and
        /// connections collection so we can use it during Edit().
        /// </summary>
        private readonly IDTSComponentMetaData100 _component;
        private readonly IServiceProvider _serviceProvider;
        private readonly Connections _connections;
        private Variables _variables;
        private ConnectionManager _dqsConnection;
        private string _serverName;
        private DataTable _dtInputColumns;
        private DataTable _dtDomains;
        private DataTable _dtOrigDomains;
        private long _currentSession;
        private const string DatabaseName = "DQS_MAIN";
        private string _knowledgeBase;
        private bool _loadProperties;
        private BindingSource _bs;
     
        /// <summary>
        /// COMPONENT PROPERTIES
        /// </summary>
        private string _dqsConnectionManager;
        private string _dqsKnowledgeBase;
        private string _mappingValues;
        private string _result;
        
        /// <summary>
        /// ADVANCED PROPERTIES
        /// </summary>
        private bool _encryptConnection;
        private bool _cleanupDqsProjects;
        private int _scoreValue = 80;

        private const string FieldNotMapped = "Field not mapped.";
        private const string NoCustomOrder = "<No Custom Ordering>";

        #endregion

        #region localProperties

        /// <summary>
        /// 
        /// </summary>
        private Knowledgebase SelectedKnowledgeBase
        {
            get
            {
                Knowledgebase KB = null;
                var knowledgeBase = ProxyEntryPointFactory.GetKnowledgebaseManagementEntryPoint(_serverName, DatabaseName, _currentSession);
                {
                    var allKbs = knowledgeBase.KnowledgebaseGet();
                    foreach (var kb in allKbs)
                    {
                        if (kb.Name != _knowledgeBase) continue;
                        KB = kb;
                        break;
                    }
                }
                return KB;
            }
        }

        #endregion

        #region UI Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtsComponentMetadata"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="variables"></param>
        /// <param name="connections"></param>
        public FrmMatchingUi(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider, Variables variables, Connections connections)
        {
            InitializeComponent();
            _connections = connections;
            _component = dtsComponentMetadata;
            _serviceProvider = serviceProvider;
            _variables = variables;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMatchingUI_Load(object sender, EventArgs e)
        {

            _loadProperties = true;
            _dtInputColumns = new DataTable("InputColumns");
            _dtDomains = new DataTable(Resources.Domains);
            PopulateComponentProperties();
            _loadProperties = false;

            dgvInputColumns.Columns[1].ReadOnly = true;
            dgvInputColumns.Columns[2].ReadOnly = true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (AreAllDomainsMappedCorrectly())
            {
                SaveMappingValues();

                _component.CustomPropertyCollection["DqsKnowledgeBase"].Value = cbDQKnowledgeBase.SelectedIndex != -1 ? cbDQKnowledgeBase.Items[cbDQKnowledgeBase.SelectedIndex] : null;
                _component.CustomPropertyCollection["Result"].Value = cbResultSet.SelectedIndex != -1 ? cbResultSet.Items[cbResultSet.SelectedIndex] : "Transitive";
                _component.CustomPropertyCollection["ScoreValue"].Value = trackBar.Value.ToString();
                _component.CustomPropertyCollection["EncryptConnection"].Value = cbEncryptConnection.Checked.ToString();
                _component.CustomPropertyCollection["MappingValues"].Value = _mappingValues;
                _component.CustomPropertyCollection["CleanUpDqsProjects"].Value = cbCleanDQSProjects.Checked.ToString();

                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(Resources.Not_all_domains_have_been_assigned_or_have_been_assigned_twice, Resources.SSIS_DQS_Matching_Transformation, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Creates a new DQS connection manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, EventArgs e)
        {
            try
            {

                /// Creates a new Connection Service and opens the DQS Cleansing Connection Manager window
                var connService = (IDtsConnectionService)_serviceProvider.GetService(typeof(IDtsConnectionService));
                var created = connService.CreateConnection("DQS");

                foreach (ConnectionManager cm in created)
                {
                    cbDQConnectionManager.Items.Insert(0, cm.Name);
                }

                if (created.Count > 0)
                {
                    cbDQConnectionManager.SelectedIndex = 0;
                }
                else
                {
                    cbDQConnectionManager.SelectedIndex = -1;
                }
            }
            catch(Exception ex)
            {
                ExceptionMessageBox("An error occurred while creating a connection manager.",ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDQConnectionManager_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string name = cbDQConnectionManager.Items[cbDQConnectionManager.SelectedIndex].ToString();

                foreach (ConnectionManager cm in _connections)
                {
                    if (cm.Name == name)
                    {
                        _dqsConnection = cm;
                        _component.RuntimeConnectionCollection["DQSConnectionManager"].ConnectionManager = DtsConvert.GetExtendedInterface(_connections[cbDQConnectionManager.SelectedItem]);
                        _component.RuntimeConnectionCollection["DQSConnectionManager"].ConnectionManagerID = _connections[cbDQConnectionManager.SelectedItem].ID;
                        break;
                    }
                }

                if (_dqsConnection != null)
                {

                    InitializeProxy();

                    _currentSession = CreateClientSession(_dqsConnection, DatabaseName);                   

                    var connectionStringParameters = DataQualityConnectorFinals.GetConnectionStringParameters(_dqsConnection.ConnectionString);
                    _serverName = connectionStringParameters["ServerName"];

                    var knowledgeBase = ProxyEntryPointFactory.GetKnowledgebaseManagementEntryPoint(_serverName, DatabaseName, _currentSession);
                    {
                        var allKbs = knowledgeBase.KnowledgebaseGet();
                        cbDQKnowledgeBase.Items.Clear();
                        foreach (var kb in allKbs)
                        {
                            cbDQKnowledgeBase.Items.Add(kb.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionMessageBox("An error occurred while retrieving the connection manager.",ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDQKnowledgeBase_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                if (!_loadProperties) SaveMappingValues();

                _knowledgeBase = cbDQKnowledgeBase.Items[cbDQKnowledgeBase.SelectedIndex].ToString();
                tvMatchingRules.Nodes.Clear();
                _dtDomains.Clear();

                if (_dqsConnection != null)
                {

                    InitializeProxy(); 

                    var matchingRules = ProxyEntryPointFactory.GetMatchingEntryPoint(_serverName, DatabaseName, _currentSession, SelectedKnowledgeBase.Id);
                    {
                        var allMRules = matchingRules.MatchingRulesGet();
                        if (allMRules != null)
                        {
                            var metadata = ProxyEntryPointFactory.GetMetadataManagementEntryPoint(_serverName, DatabaseName, _currentSession, SelectedKnowledgeBase.Id);
                            var i = 0;

                            foreach (var mr in allMRules)
                            {
                                var node = new TreeNode(mr.Name) {Tag = mr.Id, ImageIndex = 0};
                                tvMatchingRules.Nodes.Add(node);
                               var colCompositeDomain = metadata.CompositeDomainGetAll();
                                
                               if (colCompositeDomain != null)
                                {
                                    foreach (var cd in colCompositeDomain)
                                    {

                                        if (AreAllCompositeDomainElementsInMatchingRule(cd.ChildDomainIds.ToArray(), mr))
                                        {
                                            var compositeDomainName = cd.Name;
                                            var compositeDomainNode = new TreeNode(compositeDomainName)
                                            {
                                                Tag = cd.Id,
                                                ImageIndex = 2,
                                                SelectedImageIndex = 2
                                            };
                                            tvMatchingRules.Nodes[i].Nodes.Add(compositeDomainNode);
                                            
                                            foreach (var domainNode in from domainId in cd.ChildDomainIds.ToArray() let domainName = metadata.DomainGetById(domainId).Name select new TreeNode(domainName)
                                            {
                                                Tag = domainId,
                                                ImageIndex = 1,
                                                SelectedImageIndex = 1
                                            })
                                            {
                                                compositeDomainNode.Nodes.Add(domainNode);
                                            }

                                            AddDomainToDataTable(cd.Id, cd.Name, true);
                                        }
                                    }
                                }

                                foreach (var domain in mr.DomainElements)
                                {
                                    string domainName;
                                    var dom = metadata.DomainGetById(domain.DomainId);

                                    if (dom == null)
                                    {
                                        var cd = metadata.CompositeDomainGetById(domain.DomainId);
                                        domainName = cd.Name;
                                        var compositeDomainName = cd.Name;
                                        var compositeDomainNode = new TreeNode(compositeDomainName)
                                        {
                                            Tag = cd.Id,
                                            ImageIndex = 2,
                                            SelectedImageIndex = 2
                                        };
                                        tvMatchingRules.Nodes[i].Nodes.Add(compositeDomainNode);

                                        AddDomainToDataTable(domain.DomainId, domainName, true);

                                        foreach (long domainId in cd.ChildDomainIds.ToArray())
                                        {
                                            var cdDomainName = metadata.DomainGetById(domainId).Name;
                                            var domainNode = new TreeNode(cdDomainName)
                                            {
                                                Tag = domainId,
                                                ImageIndex = 1,
                                                SelectedImageIndex = 1
                                            };
                                            compositeDomainNode.Nodes.Add(domainNode);

                                            AddDomainToDataTable(domainId, cdDomainName, false);

                                        }
                                    }
                                    else
                                    {
                                        domainName = metadata.DomainGetById(domain.DomainId).Name;
                                        var domainNode = new TreeNode(domainName)
                                        {
                                            Tag = domain.DomainId,
                                            ImageIndex = 1,
                                            SelectedImageIndex = 1
                                        };
                                        tvMatchingRules.Nodes[i].Nodes.Add(domainNode);
                                        AddDomainToDataTable(domain.DomainId, domainName, false);
                                    }
                                    
                                }

                                tvMatchingRules.Nodes[i].Expand();

                                i++;
                            }
                        }
                    }
                }

                if (!_loadProperties) RestoreMappingValues();
                _dtOrigDomains = _dtDomains.Copy();
            }
            catch (Exception ex)
            {
                ExceptionMessageBox("The Knowledge Base could not be loaded." + Environment.NewLine + "Please check the Knowledge Base with the DQS Client.", ex);
            }

        }

        /// <summary>
        /// Sets the SCORE_VALUE for the matching output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar_Scroll_1(object sender, EventArgs e)
        {
            lblScoreValue.Text = String.Format("{0} %", trackBar.Value.ToString());
            _scoreValue = trackBar.Value;
        }

        /// <summary>
        /// Adds or deletes an input column from the mapping data-grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvInputColumns_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    bool check = !(bool)dgvInputColumns[0, e.RowIndex].Value;
                    if (check)
                    {
                        dgvInputColumns[2, e.RowIndex].Value = String.Empty;

                        bool rowExist = false;
                        foreach (DataGridViewRow row in dgvMapping.Rows)
                        {
                            if (row.Cells[0].Value.ToString() == dgvInputColumns[1, e.RowIndex].Value.ToString())
                            {
                                rowExist = true;
                            }
                        }

                        if (!rowExist)
                        {
                            dgvMapping.Rows.Add();
                            var newRow = dgvMapping.Rows.GetLastRow(new DataGridViewElementStates());
                            dgvMapping[0, newRow].Value = dgvInputColumns[1, e.RowIndex].Value;

                            var selectedDomains = GetAllSelectedDomain();

                            var row = _dtDomains.Select(String.Format("Domain = '{0}'", dgvInputColumns[1, e.RowIndex].Value));
                            if (row.Any())
                            {
                                if (!selectedDomains.ContainsValue(dgvInputColumns[1, e.RowIndex].Value.ToString()))
                                {
                                   dgvMapping[1, newRow].Value = row[0][1].ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        dgvInputColumns[2, e.RowIndex].Value = FieldNotMapped;
                        foreach (DataGridViewRow row in dgvMapping.Rows)
                        {
                            if (row.Cells[0].Value.ToString() == dgvInputColumns[1, e.RowIndex].Value.ToString())
                            {
                                dgvMapping.Rows.Remove(row);
                                break;
                            }
                        }
                    }

                    dgvInputColumns[0, e.RowIndex].Value = check;
                    dgvInputColumns.UpdateCellValue(0, e.RowIndex);

                }
            }
            catch (Exception ex)
            {
                ExceptionMessageBox("Sorry, during mapping columns to domains, an error has occurred...", ex);
            }

        }

        /// <summary>
        /// Opens the project website on codeplex
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkCodeplex_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://ssisdqsmatching.codeplex.com");
        }

        /// <summary>
        /// Opens the website of oh22information services
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkOH22_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.oh22.is");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            Process.Start("https://ssisdqsmatching.codeplex.com/documentation");
        }

        #endregion

        #region DataGridView

        /// <summary>
        /// Creates the Mapping Data Grid view
        /// </summary>
        private void CreateMappingColumns()
        {

            var cellInput = new DataGridViewTextBoxCell();

            var cellDomains = new DataGridViewComboBoxCell
            {
                DataSource = _dtDomains,
                DisplayMember = "Domain",
                ValueMember = "Domain"
            };

            _bs = new BindingSource();
            var dv = new DataView(_dtDomains);
            _bs.DataSource = dv;

            var colInput = new DataGridViewColumn
            {
                CellTemplate = cellInput,
                HeaderText = Resources.Input_Columns,
                ReadOnly = true
            };

            var colDomain = new DataGridViewColumn {CellTemplate = cellDomains, HeaderText = Resources.Domains};

            dgvMapping.Columns.Add(colInput);
            dgvMapping.Columns.Add(colDomain);

            dgvMapping.CellBeginEdit += dgvMapping_CellBeginEdit;
            dgvMapping.CellEndEdit += dgvMapping_CellEndEdit;
            dgvMapping.DataError += dgvMapping_DataError;

        }

        #endregion

        #region DQS

        /// <summary>
        /// checks whether the connection manager is a DQS connection manager or not
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        private bool isDQSConnection(ConnectionManager cm)
        {
            return cm.CreationName.StartsWith("DQS");
        }

        /// <summary>
        /// Creates a session with the DQS server and returns the current session id
        /// </summary>
        /// <param name="connectionManager">The DQS connection manager</param>
        /// <param name="databaseName">The DQS database name</param>
        /// <returns>The current session id</returns>
        private long CreateClientSession(ConnectionManager connectionManager, string databaseName)
        {

            if (connectionManager != null)
            {
                if (databaseName != null)
                {
                    Dictionary<string, string> connectionStringParameters = DataQualityConnectorFinals.GetConnectionStringParameters(connectionManager.ConnectionString);
                    NotificationEntryPointClient notificationEntryPoint = ProxyEntryPointFactory.GetNotificationEntryPoint(connectionStringParameters["ServerName"], databaseName);
                    NotificationSessionInfo notificationSessionInfo = notificationEntryPoint.NotificationSessionCreate(ClientType.SsisClient);
                    return notificationSessionInfo.SessionId;
                }
                else
                {
                    throw new ArgumentNullException("databaseName");
                }
            }
            else
            {
                throw new ArgumentNullException("connectionManager");
            }

        }

        /// <summary>
        /// Checks if a Composite Domain is part of Matching Rule
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="matchingRule"></param>
        /// <returns></returns>
        private static bool AreAllCompositeDomainElementsInMatchingRule(long[] domainId, MatchingRule matchingRule)
        {
            const bool retValue = true;
            var domainElements = new MatchingRuleDomainElement[matchingRule.DomainElements.Count()];
            matchingRule.DomainElements.CopyTo(domainElements, 0);
            

            for (var cdI = 0; cdI < domainId.Count(); cdI++)
            {
                var result = Array.Find(domainElements, element => element.DomainId == domainId[cdI]);
                if (result == null) return false;

            }

            return retValue;
        }

        /// <summary>
        /// Returns the ID of a knowledge base based on the name 
        /// </summary>
        /// <param name="knowledgeBaseName"></param>
        /// <returns></returns>
        private long KbGetIdByName(string knowledgeBaseName)
        {
            long id;

            var knowledgeBase = ProxyEntryPointFactory.GetKnowledgebaseManagementEntryPoint(_serverName, DatabaseName, _currentSession);
            {

                cbDQKnowledgeBase.Items.Clear();
                var kb = Array.Find(knowledgeBase.KnowledgebaseGet().ToArray(), element => element.Name == knowledgeBaseName);
                id = kb.Id;

            }

            return id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool AreAllDomainsMappedCorrectly()
        {
            const bool retValue = true;
            var selectedDomains = new List<object>();

            var metadata = ProxyEntryPointFactory.GetMetadataManagementEntryPoint(_serverName, DatabaseName, _currentSession, SelectedKnowledgeBase.Id);
            
            for (var i = 0; i < dgvMapping.RowCount; i++)
            {
                if (dgvMapping.Rows[i].Cells[1].Value == null) return false;
                var domain = dgvMapping.Rows[i].Cells[1].Value.ToString();
                {
                    var cd = metadata.CompositeDomainGetByName(dgvMapping.Rows[i].Cells[1].Value.ToString());
                    if (cd == null)                
                    {
                        if (selectedDomains.IndexOf(domain) > -1) return false;
                        selectedDomains.Add(domain);
                    }
                    else
                    {
                        foreach(var dom in cd.ChildDomainIds)
                        {
                            if (selectedDomains.IndexOf(metadata.DomainGetById(dom)) >  -1) return false;
                            selectedDomains.Add(metadata.DomainGetById(dom).Name);
                        }
                    }
                }
            }

            using (var matchingRules = ProxyEntryPointFactory.GetMatchingEntryPoint(_serverName, DatabaseName, _currentSession, SelectedKnowledgeBase.Id))
            {
                var allMRules = matchingRules.MatchingRulesGet();
                if (allMRules != null)
                {
                    foreach (MatchingRule mr in allMRules)
                    {
                        foreach(var dom in mr.DomainElements)
                        {
                            var domain = metadata.DomainGetById(dom.DomainId);
                            if (domain != null)
                            {
                                if (selectedDomains.IndexOf(domain.Name) == -1) return false;
                            }
                            else
                            {
                                if (metadata.CompositeDomainGetById(dom.DomainId).ChildDomainIds.Any(id => selectedDomains.IndexOf(metadata.DomainGetById(id).Name) == -1))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                }
            }

            return retValue;
        }

        #endregion

        /// <summary>
        /// Loads the UI components in the form with appropriate values.
        /// </summary>
        private void PopulateComponentProperties()
        {

            try
            {
                if (_component.RuntimeConnectionCollection.Count > 0)
                {
                    var conn = _component.RuntimeConnectionCollection[0];
                    if (conn != null
                        && conn.ConnectionManagerID.Length > 0
                        && _connections.Contains(conn.ConnectionManagerID))
                    {
                        _dqsConnectionManager = _connections[conn.ConnectionManagerID].Name;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionMessageBox("An error occurred while retrieving the connection manager.",ex);
            }

            _dqsKnowledgeBase = (_component.CustomPropertyCollection["DqsKnowledgeBase"].Value != null ? _component.CustomPropertyCollection["DqsKnowledgeBase"].Value.ToString() :String.Empty);
            _encryptConnection = (_component.CustomPropertyCollection["EncryptConnection"].Value != null ? Convert.ToBoolean(_component.CustomPropertyCollection["EncryptConnection"].Value.ToString()) : false);
            _mappingValues = (_component.CustomPropertyCollection["MappingValues"].Value != null ? _component.CustomPropertyCollection["MappingValues"].Value.ToString() :String.Empty);
            _cleanupDqsProjects = (_component.CustomPropertyCollection["CleanUpDqsProjects"].Value != null ? Convert.ToBoolean(_component.CustomPropertyCollection["CleanUpDqsProjects"].Value.ToString()) : false);
            _result = (_component.CustomPropertyCollection["Result"].Value != null ? _component.CustomPropertyCollection["Result"].Value.ToString() : "Transitive");

            var dqcmExist = false;

            cbDQConnectionManager.Items.Clear();
            foreach (var cm in _connections.Cast<ConnectionManager>().Where(isDQSConnection))
            {
                cbDQConnectionManager.Items.Add(cm.Name);
                if (cm.Name == _dqsConnectionManager) dqcmExist = true;
            }

            if (dqcmExist)
            {
                cbDQConnectionManager.SelectedItem = cbDQConnectionManager.Items[cbDQConnectionManager.Items.IndexOf(_dqsConnectionManager)];
            }

            //Fill a DataTable with Input Columns
            PopulateInputDataTable();

            ///Create a DataTable for domain vlaues
            CreateDomainTable();

            ///Create columns as templates for mapping DataGridView
            CreateMappingColumns();

            ///Create values for Input Column DataGridView
            CreateInputColumns();

            var sortTable = _dtInputColumns.Copy();
            var doNotSortRow = sortTable.NewRow();
            doNotSortRow[0] = false;
            doNotSortRow[1] = NoCustomOrder;
            sortTable.Rows.Add(doNotSortRow);

            if (_component.CustomPropertyCollection["ScoreValue"].Value != null)
            {
                int trackBarValue = Convert.ToInt32(_component.CustomPropertyCollection["ScoreValue"].Value.ToString());
                if (trackBarValue < 50 || trackBarValue > 100)
                {
                    trackBar.Value = 80;
                }
                else
                {
                    trackBar.Value = trackBarValue;
                }
            }
            else
            {
                trackBar.Value = 80;
            }
            
            lblScoreValue.Text = String.Format("{0} %", trackBar.Value.ToString());

            try
            {
                cbDQKnowledgeBase.SelectedItem = cbDQKnowledgeBase.Items[cbDQKnowledgeBase.Items.IndexOf(_dqsKnowledgeBase)];
            }
            catch
            {
                // ignored
            }

            try
            {
                cbResultSet.SelectedItem = cbResultSet.Items[cbResultSet.Items.IndexOf(_result)];
            }
            catch
            {
                // ignored
            }

            cbEncryptConnection.Checked = _encryptConnection;
            cbCleanDQSProjects.Checked = _cleanupDqsProjects;

            if (!String.IsNullOrEmpty(_mappingValues))
            {
                RestoreMappingValues();
            }
                      
        }

        /// <summary>
        /// Creates the _dtDomain data table
        /// </summary>
        private void CreateDomainTable()
        {
            var domainId = new DataColumn("ID", Type.GetType("System.Int32"));
            _dtDomains.Columns.Add(domainId);

            var domainColumns = new DataColumn("Domain", Type.GetType("System.String"));
            _dtDomains.Columns.Add(domainColumns);

            var isCompositeDOmain = new DataColumn("IsCompositeDomain", Type.GetType("System.Boolean"));
            _dtDomains.Columns.Add(isCompositeDOmain);          

        }

        /// <summary>
        /// Creates the Iinput column datagrid
        /// </summary>
        private void CreateInputColumns()
        {
            dgvInputColumns.DataSource = _dtInputColumns.Copy();
            dgvInputColumns.Columns[0].HeaderText = "";
            dgvInputColumns.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvInputColumns.Columns[0].Width = 50;
            dgvInputColumns.Columns[1].HeaderText = Resources.Input_Columns;
            dgvInputColumns.Columns[2].HeaderText = Resources.Mapping_Info;
        }

        /// <summary>
        /// Creates a data table of all input columns and bind the data table to the input data grid.
        /// </summary>
        private void PopulateInputDataTable()
        {
            _dtInputColumns.Clear();

            using (var activeColumn = new DataColumn("ActiveColumns", Type.GetType("System.Boolean")))
            {
                _dtInputColumns.Columns.Add(activeColumn);
            }

            var inputColumn = new DataColumn("InputColumns", Type.GetType("System.String"));
            _dtInputColumns.Columns.Add(inputColumn);

            var mappingInfoColumn = new DataColumn("Info", Type.GetType("System.String"));
            _dtInputColumns.Columns.Add(mappingInfoColumn);

            var inputColumns = (_mappingValues != null ? _mappingValues.Split(';') : null);

            for (var i = 0; i < _component.InputCollection.Count; i++)
            {
                _component.InputCollection[i].InputColumnCollection.RemoveAll();
                var input = _component.InputCollection[i].GetVirtualInput();
                foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
                {
                    var row = _dtInputColumns.NewRow();
                    var activeColumn = false;

                    if (inputColumns != null)
                    { 
                        foreach (var str in inputColumns)
                        {
                            if (str.Split('|')[0] == vcol.Name && !String.IsNullOrEmpty(str.Split('|')[1])) activeColumn = true;
                        }
                    }
                    
                    row[0] = activeColumn;
                    row[1] = vcol.Name;

                    if (activeColumn == false)
                    {
                        row[2] = FieldNotMapped;
                    }
                    else
                    {
                        row[2] = string.Empty;
                    }

                    _dtInputColumns.Rows.Add(row);
                }
            }
        }

        /// <summary>
        /// Adds a domain to the domain data table.
        /// The domain data table is used for the drop down list in the domain mapping.
        /// </summary>
        /// <param name="id">The Id of the domain</param>
        /// <param name="name">The Name of the domain</param>
        /// <param name="isCompositeDomain">Indicates if the domain is a composite domain or not</param>
        private void AddDomainToDataTable(long id, string name, bool isCompositeDomain)
        {
            if (!_dtDomains.Select(String.Format("Id = {0}", id)).Any())
            {
                var row = _dtDomains.NewRow();
                row[0] = id;
                row[1] = name;
                row[2] = isCompositeDomain;
                _dtDomains.Rows.Add(row);
            }

        }
        
        /// <summary>
        /// All mapped input columns and domains will saved as a pair
        /// ==> INPUT_COLUMN | DOMAIN
        /// Different mappings are divided by a semicolon
        /// </summary>
        private void SaveMappingValues()
        {
            _mappingValues = null;

            if (dgvMapping.RowCount > 0)
            {
                foreach (DataRow row in _dtInputColumns.Rows)
                {
                    var input = row[1].ToString();

                    for (var i = 0; i < dgvMapping.RowCount; i++)
                    {
                        if (input == dgvMapping.Rows[i].Cells[0].Value.ToString())
                        {
                            if (dgvMapping.Rows[i].Cells[1].Value != null)
                            {
                                _mappingValues += String.Format("{0}|{1};", input, dgvMapping.Rows[i].Cells[1].Value);
                                break;
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(_mappingValues)) _mappingValues = _mappingValues.Substring(0, (_mappingValues.Length - 1));

            }

        }

        /// <summary>
        /// Restores the mapping value
        /// Adds the input columns to the datagrid and map them to the domains
        /// </summary>
        private void RestoreMappingValues()
        {
          
            dgvMapping.Rows.Clear();

            foreach (DataGridViewRow dgvRow in dgvInputColumns.Rows)
            {
                dgvRow.Cells[0].Value = false;
                dgvRow.Cells[2].Value = FieldNotMapped;
            }

            if (_mappingValues != null)
            {
                var mappingRows = _mappingValues.Split(';');
                for (var i = 0; i < mappingRows.Count(); i++)
                {

                    var setValue = false;
                    var mappingColumns = mappingRows[i].Split('|');

                    if (!String.IsNullOrEmpty(mappingColumns[1])
                        && !String.IsNullOrEmpty(mappingColumns[0])
                        && _dtDomains.Select(String.Format("Domain = '{0}'", mappingColumns[1])).Any()
                        && _dtInputColumns.Select(String.Format("InputColumns = '{0}'", mappingColumns[0])).Any())
                    {

                        dgvMapping.Rows.Add();
                        var lastRow = dgvMapping.Rows.GetLastRow(new DataGridViewElementStates());

                        if (_dtDomains.Select(String.Format("Domain = '{0}'", mappingColumns[1])).Any())
                        {
                            dgvMapping.Rows[lastRow].Cells[1].Value = mappingColumns[1];
                            setValue = true;
                        }

                        if (_dtInputColumns.Select(String.Format("InputColumns = '{0}'", mappingColumns[0])).Any())
                        {
                            dgvMapping.Rows[lastRow].Cells[0].Value = mappingColumns[0];
                            setValue = true;
                        }

                        foreach (DataGridViewRow dgvRow in dgvInputColumns.Rows.Cast<DataGridViewRow>().Where(dgvRow => dgvRow.Cells[1].Value.ToString() == mappingColumns[1]))
                        {
                            dgvRow.Cells[0].Value = true;
                            dgvRow.Cells[2].Value = "";
                        }

                        if (!setValue) dgvMapping.Rows.RemoveAt(dgvMapping.Rows.Count - 1);

                    }
                    else
                    {
                        foreach (DataGridViewRow dgvRow in dgvInputColumns.Rows)
                        {
                            if (dgvRow.Cells[1].Value.ToString() == mappingColumns[0])
                            {
                                dgvRow.Cells[0].Value = false;
                                dgvRow.Cells[2].Value = FieldNotMapped;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connName"></param>
        /// <returns></returns>
        private IDTSRuntimeConnection100 GetRuntimeConn(string connName)
        {
            try
            {
                var conn = _component.RuntimeConnectionCollection[connName];
                return conn;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeProxy()
        {
            try
            {
                ProxyInit.UseEncryption = cbEncryptConnection.Checked;
                if (!ProxyInit.IsInit)
                {
                    ProxyInit.Init(new SsisProxyInitParameters());
                }
                
            }
            catch (Exception ex)
            {
                ExceptionMessageBox("The Proxy could not be initialized.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userFriendlyMessage"></param>
        /// <param name="ex"></param>
        private void ExceptionMessageBox(string userFriendlyMessage, Exception ex)
        {
            var exTop = new ApplicationException(userFriendlyMessage, ex) {Source = Text};
            var box = new ExceptionMessageBox(exTop, ExceptionMessageBoxButtons.OK, ExceptionMessageBoxSymbol.Error);
            box.Show(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvMapping_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                // Set the combobox cell datasource to the filtered BindingSource 
                var dgcb = (DataGridViewComboBoxCell)dgvMapping[e.ColumnIndex, e.RowIndex];
                dgcb.DataSource = _bs;
                var ht = GetAllSelectedDomain();

                var selectedValue = (dgvMapping.Rows[e.RowIndex].Cells[1].Value == null ? "" : dgvMapping.Rows[e.RowIndex].Cells[1].Value.ToString());

                var strFilter = ht.Values.Cast<object>().Where(val => val.ToString() != selectedValue).Aggregate("", (current, val) => current + String.Format("'{0}',", val.ToString()));

                if (strFilter.Trim().Length != 0)
                {
                    strFilter = strFilter.Substring(0, strFilter.Length - 1);
                    _bs.Filter = "Domain not in (" + strFilter + ")";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvMapping_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                // Reset combobox cell to the unfiltered BindingSource 
                var dgcb = (DataGridViewComboBoxCell)dgvMapping[e.ColumnIndex, e.RowIndex];
                dgcb.DataSource = _dtDomains;
                _bs.RemoveFilter();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Hashtable GetAllSelectedDomain()
        {
            Hashtable hashTable = new Hashtable();

            using (var metadata = ProxyEntryPointFactory.GetMetadataManagementEntryPoint(_serverName, DatabaseName, _currentSession, SelectedKnowledgeBase.Id))
            {
                for (var i = 0; i < dgvMapping.Rows.Count; i++)
                {
                    if (dgvMapping.Rows[i].Cells[1].Value != null)
                    {
                        var selectFilter = String.Format("Domain = '{0}'", dgvMapping.Rows[i].Cells[1].Value.ToString());
                        var row = _dtDomains.Select(selectFilter);
                        if (row.Any())
                        {
                            if (Convert.ToBoolean(row[0][2]) == false)
                            {
                                if (!hashTable.ContainsKey(row[0][0]))
                                {
                                    hashTable.Add(row[0][0], row[0][1]);
                                }
                            }
                            else
                            {
                                if (!hashTable.ContainsKey((long)Convert.ToDouble(row[0][0])))
                                {
                                    hashTable.Add((long)Convert.ToDouble(row[0][0]), row[0][1]);
                                }

                                var childDomainIds = metadata.CompositeDomainGetById((long)Convert.ToDouble(row[0][0])).ChildDomainIds.ToArray();
                                for (var m = 0; m < childDomainIds.Count(); m++)
                                {
                                    if (!hashTable.ContainsKey(childDomainIds[m]))
                                    {
                                        hashTable.Add(childDomainIds[m], metadata.DomainGetById(childDomainIds[m]).Name);
                                    }
                                }
                            }

                            var compositeDomain = metadata.CompositeDomainGetAll();
                            if (compositeDomain != null)
                            {
                                foreach (var cd in compositeDomain)
                                {
                                    var result = Array.Find(cd.ChildDomainIds.ToArray(), element => element == (long)Convert.ToDouble(row[0][0]));
                                    if (result != 0)
                                    {
                                        if (!hashTable.ContainsKey(cd.Id))
                                        {
                                            hashTable.Add(cd.Id, cd.Name);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

            return hashTable;

        }

        private void dgvMapping_DataError(object sender, DataGridViewDataErrorEventArgs anError) {}

    }
}
