using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using Microsoft.Ssdqs.Component.Common.Logic;
using Microsoft.Ssdqs.Component.Common.Messages;
using Microsoft.Ssdqs.Component.Common.Utilities;
using Microsoft.Ssdqs.Core.Service.Export;
using Microsoft.Ssdqs.Core.Service.KnowledgebaseManagement.Define;
using Microsoft.Ssdqs.Core.Service.Process;
using Microsoft.Ssdqs.Core.Service.Requests.DataService;
using Microsoft.Ssdqs.Core.TypedCollections;
using Microsoft.Ssdqs.EntryPoint;
using Microsoft.Ssdqs.Flow.Notification;
using Microsoft.Ssdqs.Infra.Codes;
using Microsoft.Ssdqs.Matching.Define;
using Microsoft.Ssdqs.Proxy.EntryPoint;
#pragma warning disable 1587

namespace oh22is.SqlServer.DQS
{
#if SQL2012
    [DtsPipelineComponent(
        DisplayName = "DQS Matching",
        Description = "SSIS DQS Matching Transformation",
        IconResource = "oh22is.SqlServer.DQS.Matching.ico",
        UITypeName = "oh22is.SqlServer.DQS.MatchingUI, oh22is.SqlServer.DQS.MatchingUI, Version=1.2.0.0, Culture=neutral, PublicKeyToken=daba57ceffa2385b",
        ComponentType = ComponentType.Transform,
        CurrentVersion = 0)]
#elif SQL2014
    [DtsPipelineComponent(
        DisplayName = "DQS Matching",
        Description = "SSIS DQS Matching Transformation",
        IconResource = "oh22is.SqlServer.DQS.Matching.ico",
        UITypeName = "oh22is.SqlServer.DQS.MatchingUI, oh22is.SqlServer2014.DQS.MatchingUI, Version=1.2.0.0, Culture=neutral, PublicKeyToken=daba57ceffa2385b",
        ComponentType = ComponentType.Transform,
        CurrentVersion = 0)]
#elif SQL2016
    [DtsPipelineComponent(
        DisplayName = "DQS Matching",
        Description = "SSIS DQS Matching Transformation",
        IconResource = "oh22is.SqlServer.DQS.Matching.ico",
        UITypeName = "oh22is.SqlServer.DQS.MatchingUI, oh22is.SqlServer2016.DQS.MatchingUI, Version=1.2.0.0, Culture=neutral, PublicKeyToken=daba57ceffa2385b",
        ComponentType = ComponentType.Transform,
        CurrentVersion = 0)]
#elif SQL2017
    [DtsPipelineComponent(
        DisplayName = "DQS Matching",
        Description = "SSIS DQS Matching Transformation",
        IconResource = "oh22is.SqlServer.DQS.Matching.ico",
        UITypeName = "oh22is.SqlServer.DQS.MatchingUI, oh22is.SqlServer2017.DQS.MatchingUI, Version=1.2.0.0, Culture=neutral, PublicKeyToken=daba57ceffa2385b",
        ComponentType = ComponentType.Transform,
        CurrentVersion = 0)]
#endif
    public class Matching : PipelineComponent
    {

        #region Private Variables

        /// <summary>
        /// COMPONENT PROPERTIES
        /// </summary>
        private IDTSCustomProperty100 _idtsDqsKnowledgeBase;
        private IDTSCustomProperty100 _idtsEncryptConnection;
        private IDTSCustomProperty100 _idtsMappingValues;
        private IDTSCustomProperty100 _idtsCleanupDqsProjects;
        private IDTSCustomProperty100 _idtsScoreValue;

        public enum Result
        {
            NonOverlappingClusters,
            OverlappingClusters
        }

        private const string DatabaseName = "DQS_MAIN";
        private DQProject _dqsProject;
        private string _serverName;
        private ConnectionManager _dqsConnection;
        private DataSourceMapping _dataSourceMapping;
        private PipelineBuffer _matchedOutputBuffer;
        private PipelineBuffer _unmatchedOutputBuffer;
        private PipelineBuffer _survivorshipOutputBuffer;
        private string _result;
        private int _chunkSize;
        private long _knowledgeBaseId;
        private string _knowledgeBaseName;
        private int _minConfidence;
        private NotificationSessionInfo _currentSession;
        private int _matchedRecordCount;
        private int _unmatchedRecordCount;
        private int _survivorshipRecordCount;

        /// <summary>
        /// Number of additional columns for the matched output
        /// </summary>
        private const int AdditionalColumns = 10;
    
        /// <summary>
        /// Used when an Event needs to be canceled.
        /// </summary>
        private bool _cancelEvent = true;

        /// <summary>
        /// An array list of all input columns
        /// </summary>
        private ArrayList _columns;

        #endregion

        #region Component Properties

        /// <summary>
        /// Called when the component is initally added to a data flow task. 
        /// Create and configure the input and outputs of the component.
        /// </summary>
        public override void ProvideComponentProperties()
        {

            // Reset the component.
            RemoveAllInputsOutputsAndCustomProperties();
            ComponentMetaData.RuntimeConnectionCollection.RemoveAll();

            var input = ComponentMetaData.InputCollection.New();
            input.Name = "Input";
            input.HasSideEffects = true;

            // Add Custom Properties
            AddUserProperties();

            // Connection Manager
            var connectionManager = ComponentMetaData.RuntimeConnectionCollection.New();
            connectionManager.Name = "DQSConnectionManager";
            

            // Matched Output
            var matchedOutput = ComponentMetaData.OutputCollection.New();
            matchedOutput.Name = "Matched Output";
            matchedOutput.Description = "Matched output rows are directed to this output.";
            matchedOutput.SynchronousInputID = 0;

            // Unmatched Output
            var unmatchedOutput = ComponentMetaData.OutputCollection.New();
            unmatchedOutput.Name = "Unmatched Output";
            unmatchedOutput.Description = "Unmatched output rows are directed to this output.";
            unmatchedOutput.SynchronousInputID = 0;

            // Unmatched Output
            //var survivorshipOutput = ComponentMetaData.OutputCollection.New();
            //survivorshipOutput.Name = "Survivorship Output";
            //survivorshipOutput.Description = "Survivorship output rows are directed to this output.";
            //survivorshipOutput.SynchronousInputID = 0;

        }

        /// <summary>
        /// Adds user properties
        /// </summary>
        private void AddUserProperties()
        {
            
            /// DQS Knowledge Base
            _idtsDqsKnowledgeBase = ComponentMetaData.CustomPropertyCollection.New();
            _idtsDqsKnowledgeBase.Name = "DqsKnowledgeBase";
            _idtsDqsKnowledgeBase.Description = "DqsKnowledgeBase";
            _idtsDqsKnowledgeBase.ExpressionType = DTSCustomPropertyExpressionType.CPET_NONE;

            /// DQS Result
            _idtsDqsKnowledgeBase = ComponentMetaData.CustomPropertyCollection.New();
            _idtsDqsKnowledgeBase.Name = "Result";
            _idtsDqsKnowledgeBase.Description = "Result";
            _idtsDqsKnowledgeBase.ExpressionType = DTSCustomPropertyExpressionType.CPET_NONE;

            /// Mapping between input columns and domains.
            /// Stored as COLUMN | DOMAIN ; COLUMN | DOMAIN 
            _idtsMappingValues = ComponentMetaData.CustomPropertyCollection.New();
            _idtsMappingValues.Name = "MappingValues";
            _idtsMappingValues.Description = "MappingValues";
            _idtsMappingValues.ExpressionType = DTSCustomPropertyExpressionType.CPET_NONE;

            /// Encrypt Connection
            _idtsEncryptConnection = ComponentMetaData.CustomPropertyCollection.New();
            _idtsEncryptConnection.Name = "EncryptConnection";
            _idtsEncryptConnection.Description = "EncryptConnection";
            _idtsEncryptConnection.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            _idtsEncryptConnection.TypeConverter = typeof(Boolean).AssemblyQualifiedName;
            _idtsEncryptConnection.Value = false;

            /// Clean up DQS Projects after execution
            /// Currently this feature is not supported
            _idtsCleanupDqsProjects = ComponentMetaData.CustomPropertyCollection.New();
            _idtsCleanupDqsProjects.Name = "CleanUpDqsProjects";
            _idtsCleanupDqsProjects.Description = "CleanUpDqsProjects";
            _idtsCleanupDqsProjects.TypeConverter = typeof(Boolean).AssemblyQualifiedName;
            _idtsCleanupDqsProjects.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;
            _idtsCleanupDqsProjects.Value = true;

            /// Minimum Score value for the output
            /// Duplicates below the scores will not send to the unmatched output
            _idtsScoreValue = ComponentMetaData.CustomPropertyCollection.New();
            _idtsScoreValue.Name = "ScoreValue";
            _idtsScoreValue.Description = "ScoreValue";
            _idtsScoreValue.ExpressionType = DTSCustomPropertyExpressionType.CPET_NOTIFY;

        }

        #endregion

        #region ReinitializeMetaData
        
        /// <summary>
        /// 
        /// </summary>
        public override void ReinitializeMetaData()
        {

            if (ComponentMetaData.InputCollection.Count > 0)
            {

                var input = ComponentMetaData.InputCollection[0];
                var virtualInput = input.GetVirtualInput();
                var matchedOutput = ComponentMetaData.OutputCollection[0];
                var unmatchedOutput = ComponentMetaData.OutputCollection[1];
                //var survivorshipOutput = ComponentMetaData.OutputCollection[2];

                /// Reset the component
                matchedOutput.OutputColumnCollection.RemoveAll();
                matchedOutput.ExternalMetadataColumnCollection.RemoveAll();
                matchedOutput.IsSorted = false;

                unmatchedOutput.OutputColumnCollection.RemoveAll();
                unmatchedOutput.ExternalMetadataColumnCollection.RemoveAll();
                unmatchedOutput.IsSorted = false;

                //survivorshipOutput.OutputColumnCollection.RemoveAll();
                //survivorshipOutput.ExternalMetadataColumnCollection.RemoveAll();
                //survivorshipOutput.IsSorted = false;

                try
                {

                    /// Create special DQS specific columns for the matched output
                    var matchedOutputColNew = BuildColumn(matchedOutput, "RecordId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "ClusterId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "ClusterRecordRelationId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "MatchingScore", DataType.DT_R8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "RuleId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "IsPivot", DataType.DT_BOOL, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "Status", DataType.DT_WSTR, 255, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "PairId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "SiblingId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                    matchedOutputColNew = BuildColumn(matchedOutput, "PivotId", DataType.DT_I8, 0, 0, 0, 0, "");
                    CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);


                    /// Create a output column for each input column
                    /// and related external metadata columns
                    foreach (IDTSVirtualInputColumn100 column in virtualInput.VirtualInputColumnCollection)
                    {

                        matchedOutputColNew = BuildColumn(matchedOutput, column);
                        CreateExternalMetaDataColumn(matchedOutput.ExternalMetadataColumnCollection, matchedOutputColNew);

                        var unmatchedOutputColNew = BuildColumn(unmatchedOutput, column);
                        CreateExternalMetaDataColumn(unmatchedOutput.ExternalMetadataColumnCollection, unmatchedOutputColNew);

                        //survivorshipOutputColNew = BuildColumn(survivorshipOutput, column);
                        //CreateExternalMetaDataColumn(survivorshipOutput.ExternalMetadataColumnCollection, survivorshipOutputColNew);

                        //virtualInput.SetUsageType(column.LineageID, DTSUsageType.UT_READONLY);
                        
                    }

                }
                catch
                {
                    ComponentUtility.FireError(ComponentMetaData, ComponentMessage.DataCorrectionInputContainsInvalidColumns);
                }
            }

            base.ReinitializeMetaData();

        }

        #endregion

        #region PreExecute

        /// <summary>
        /// PreExecute is called one time per component
        /// </summary>
        public override void PreExecute()
        {
            /// Saves the current input column in a ArraList for later data mapping
            /// Not the best place, must change it later
            var vInput = ComponentMetaData.InputCollection[0].GetVirtualInput();
            _columns = new ArrayList();
            foreach (IDTSVirtualInputColumn100 vcol in vInput.VirtualInputColumnCollection)
            {
                _columns.Add(vcol.Name);
            }

            try
            {
                if (_dqsConnection != null)
                {

                    /// Initilize the DQS Proxy
                    InitializeProxy();

                    /// Defines different settings for the DQS Project
                    var connectionStringParameters = DataQualityConnectorFinals.GetConnectionStringParameters(_dqsConnection.ConnectionString);
                    _serverName = (connectionStringParameters["ServerName"] == "." ? "localhost" : connectionStringParameters["ServerName"]);
                    
                    /// Porjectname composed of component name and a GUID
                    var objArray = new object[] { ComponentMetaData.Name, Guid.NewGuid().ToString("N") }; 
                    var projectName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", objArray);
                    const string description = "This project was created by the SSIS oh22is.DQS.Matching component.";

                    /// Set the minimal confidence score, based on the threshold slider on the UI
                    _minConfidence = Convert.ToInt32(ComponentMetaData.CustomPropertyCollection["ScoreValue"].Value.ToString());

                    /// Cretate the DQS Client session
                    _currentSession = CreateClientSession();

                    // Create the DQS porject in the current session
                    using (var knowledgeBase = ComponentUtility.GetKnowledgebaseManagementEntryPoint(ComponentMetaData, DatabaseName, _currentSession.SessionId))
                    {
                        
                        _knowledgeBaseId = GetKnowledgeBaseIdByName(ComponentMetaData.CustomPropertyCollection["DqsKnowledgeBase"].Value.ToString());
                        _knowledgeBaseName = ComponentMetaData.CustomPropertyCollection["DqsKnowledgeBase"].Value.ToString();
                        _result = ComponentMetaData.CustomPropertyCollection["Result"].Value.ToString();

                        var allKbs = knowledgeBase.KnowledgebaseGet();
                        var kb = Array.Find(allKbs.ToArray(), element => element.Name == _knowledgeBaseName);
                        if (kb == null)
                        {
                            InternalFireError("The Knowledge Base does not exist.");
                        }

                        _dqsProject = knowledgeBase.DQProjectAdd(projectName, description, _knowledgeBaseId);

                        _dqsProject.InWorkPhase = DQProjectInWorkPhase.MatchingDataSourceMapping;
                        _dqsProject = knowledgeBase.DQProjectOpen(_dqsProject);
                    }

                }

                /// load Chunksize from DQS
                using (var dataQualityEntryPoint = ComponentUtility.GetDataQualityEntryPoint(ComponentMetaData, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
                {
                    var strs = dataQualityEntryPoint.Parameters();
                    _chunkSize = strs["DCChunkSize"];
                }

                _dataSourceMapping = null;

            }
            catch (Exception ex)
            {
                InternalFireError(ex.ToString());
            }
        }

        #endregion

        #region PrimeOutput

        /// <summary>
        /// Called at run time for components with asynchronous 
        /// outputs to let these components add rows to the output buffers.
        /// </summary>
        /// <param name="outputs"></param>
        /// <param name="outputIDs"></param>
        /// <param name="buffers"></param>
        public override void PrimeOutput(int outputs, int[] outputIDs, PipelineBuffer[] buffers)
        {
            
            if (buffers == null) throw new ArgumentNullException("buffers");
            
            _matchedOutputBuffer = buffers[0];
            _unmatchedOutputBuffer = buffers[1];
            //_survivorshipOutputBuffer = buffers[2];

        }

        #endregion

        #region ProcessInput

        /// <summary>
        /// This method is called repeatedly during package execution. 
        /// It is called each time the data flow task has a full buffer provided by an upstream component. 
        /// </summary>
        /// <param name="inputId">The ID of the input object of the component.</param>
        /// <param name="buffer">The input buffer.</param>
        public override void ProcessInput(int inputId, PipelineBuffer buffer)
        {
            /// As long as the buffer provides data
            if (!buffer.EndOfRowset)
            {
                try
                {
                    
                    /// Create a ReadOnlyString Collection to upload incoming data to DQS
                    var lRec = new List<ReadOnlyStringCollection>();

                    while (buffer.NextRow())
                    {
                        if (!buffer.EndOfRowset)
                        {
                            
                            var list = new List<string>();
                            try
                            {
                                for (int i = 0; i < buffer.ColumnCount; i++)
                                {
                                    /// Add each buffer column to the list
                                    list.Add((buffer.IsNull(i) ? String.Empty : buffer[i].ToString()));
                                }

                                /// Add the list to theReadOnlyStringCollection
                                lRec.Add(new ReadOnlyStringCollection(list));

                            }
                            catch (Exception ex)
                            {
                                InternalFireError(lRec.Count.ToString());
                                InternalFireError(ex.ToString());
                            }
                        }

                    }

                    if (lRec.Count > 0)
                    {
                        /// Upload records to DQS
                        UploadRecords(lRec);
                        InternalFireInformation(String.Format("{0} records uploaded.", lRec.Count()));
                        lRec.Clear();
                    }

                }
                catch (Exception ex)
                {
                    /// If an error occurred, the project will be directly deleted.
                    DeleteDqsProject();
                    InternalFireError(ex.ToString());
                }
            }
            else
            {

                /// The matching process is started
                if (StartProcess())
                {

                    /// Send information to the log
                    InternalFireInformation("DQS matching phase is beginning.");

                    #region Matching Results Summary Stats

                    /// Write the last process summary to the log
                    WriteLastProcessSummary();

                    #endregion

                    /// If the output exists, the matched data of DQS will be retrieved in a separate thread.
                    var matchedThread = new Thread(MatchedOutput);
                    if (ComponentMetaData.OutputCollection[0].IsAttached) matchedThread.Start();

                    /// If the output exists, the unmatched data of DQS will be retrieved in a separate thread.
                    var unmatchedThread = new Thread(UnmatchedOutput);
                    if (ComponentMetaData.OutputCollection[1].IsAttached) unmatchedThread.Start();

                    /// If the output exists, the unmatched data of DQS will be retrieved in a separate thread.
                    //var survivorshipThread = new Thread(SurvivorshipOutput);
                    //if (ComponentMetaData.OutputCollection[2].IsAttached) survivorshipThread.Start();

                    /// Check that the threads are still running                    
                    while (matchedThread.IsAlive) { }
                    if (_matchedOutputBuffer.EndOfRowset == false) _matchedOutputBuffer.SetEndOfRowset();

                    while (unmatchedThread.IsAlive) { }
                    if (_unmatchedOutputBuffer.EndOfRowset == false) _unmatchedOutputBuffer.SetEndOfRowset();

                    //while (survivorshipThread.IsAlive) { }
                    //if (_survivorshipOutputBuffer.EndOfRowset == false) _survivorshipOutputBuffer.SetEndOfRowset();

                    /// Update the DQS project status to MatchingExport
                    /// Actually, this should give the opportunity to open the project in the DQS client.
                    /// Currently, this is not supported by the component.
                    /// The project will be deleted after execution.
                    _dqsProject.InWorkPhase = DQProjectInWorkPhase.MatchingExport;
                    UpdateDqsProject();

                }

            }

            base.ProcessInput(inputId, buffer);
        }

        #endregion

        #region Matching Process

        /// <summary>
        ///  Starts the matching process in the DQS
        /// </summary>
        /// <returns></returns>
        private bool StartProcess()
        {

            bool retValue;

            /// GetDataQualityProcessManagementEntryPoint
            using (var process = ProxyEntryPointFactory.GetDataQualityProcessManagementEntryPoint(_serverName, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
            {

                var matchingClusteringOption = MatchingClusteringOption.None;
                switch (_result)
                {
                    case "Transitive":
                        matchingClusteringOption = MatchingClusteringOption.Transitive;
                        break;
                    case "Raw":
                        matchingClusteringOption = MatchingClusteringOption.Raw;
                        break;
                    case "None":
                        matchingClusteringOption = MatchingClusteringOption.None;
                        break;
                }

                /// Starts the matching process in the DQS
                /// ProcessID is stored in a local variable
                var processId = process.ProcessStart(ProcessType.Matching, _dataSourceMapping.Id.ToString(CultureInfo.InvariantCulture) + ',' + "null" + ',' + ((byte)matchingClusteringOption).ToString(CultureInfo.InvariantCulture));

                /// Checks if the process is still running
                while (process.ProcessIsRunning(processId)) { }

                /// Get the current process from DQS
                /// If the status is successful, additional process information is written to the log.
                /// Otherwise process error and reason are written to the log; the execution is cancelled.
                DataQualityProcess proc = process.ProcessGet(processId);
                if (proc.Status != CoreStatus.Success)
                {
                    InternalFireError(proc.StatusError);
                    InternalFireError(proc.StatusReason);
                }
                else
                {
                    InternalFireInformation(String.Format("CreationTime: {0}", proc.CreationTime));
                    InternalFireInformation(String.Format("Start Time: {0}", proc.StartTime));
                    InternalFireInformation(String.Format("EndTime: {0}", proc.EndTime));
                    InternalFireInformation(String.Format("ElapsedTime: {0}", proc.ElapsedTime));
                    InternalFireInformation(String.Format("Results: {0}", proc.Results));
                }

                retValue = true;
            }

            return retValue;

        }

        /// <summary>
        /// 
        /// </summary>
        private void MatchedOutput()
        {
            #region Matched Output

            /// GetMatchingEntryPoint
            using (var matching = ProxyEntryPointFactory.GetMatchingEntryPoint(_serverName, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
            {

                /// If duplicates were found
                if (_matchedRecordCount > 0)
                {

                    /// calculate the page size
                    var maxPageNumber = ((int)Math.Ceiling((float)_matchedRecordCount / (float)_chunkSize)) + 1;
                    var pageSize = _chunkSize;
                    const bool isDescendingSorting = false;

                    const MatchingResultType resultType = MatchingResultType.Matched;
                    const bool allCollapsed = false;
                    var collapsedExpandedCluster = new Collection<long>();

                    var pageNumber = 0;
                    var resultsMatchedRecords = _chunkSize;

                    while (resultsMatchedRecords > 0)
                    {
                        /// Define the page and load it with the given datasource mapping id.
                        var pageInfo = new MatchingResultsPageInfo(pageNumber, pageSize, null, isDescendingSorting, _minConfidence, resultType, allCollapsed, collapsedExpandedCluster);
                        var resultsMatched = matching.MatchingResultsGetPage(pageInfo, _dataSourceMapping.Id);

                        var listColumnNameAndTypes = resultsMatched.DataColumnNamesAndTypes.ToList();

                        resultsMatchedRecords = resultsMatched.Records.Count();

                        /// Write the records to the output buffer
                        foreach (var record in resultsMatched.Records)
                        {

                            var k = 0;

                            _matchedOutputBuffer.AddRow();
                            _matchedOutputBuffer.SetInt64(k++, Convert.ToInt64(record.RecordId));
                            _matchedOutputBuffer.SetInt64(k++, Convert.ToInt64(record.ClusterId));
                            _matchedOutputBuffer.SetInt64(k++, Convert.ToInt64(record.ClusterRecordRelationId));
                            _matchedOutputBuffer.SetDouble(k++, Convert.ToDouble(record.MatchingScore));
                            _matchedOutputBuffer.SetInt64(k++, Convert.ToInt64(record.RuleId));
                            _matchedOutputBuffer.SetBoolean(k++, Convert.ToBoolean(record.IsPivot));
                            _matchedOutputBuffer.SetString(k++, record.Status.ToString());
                            _matchedOutputBuffer.SetInt64(k++, Convert.ToInt64(record.PairId));
                            _matchedOutputBuffer.SetInt64(k++, Convert.ToInt64(record.SiblingId));
                            _matchedOutputBuffer.SetInt64(k, Convert.ToInt64(record.PivotId));

                            var i = 0;
                            foreach (var values in record.DataValues)
                            {
                                try
                                {
                                    AddValueToBuffer(_matchedOutputBuffer, GetOutputColumn(0, listColumnNameAndTypes[i].Name), 
                                        values, GetOutputDataType(0, listColumnNameAndTypes[i].Name));
                                    
                                    i++;
                                }
                                catch (Exception ex)
                                {
                                    _matchedOutputBuffer[i] = null;
                                    i++;
                                    InternalFireError(ex.ToString());
                                }
                            }
                        }

                        /// write a progress to thee log
                        InternalFireProgress("Matched records are written.", (100 / maxPageNumber * pageNumber));
                        pageNumber++;
                    }
                    _matchedOutputBuffer.SetEndOfRowset();
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private void UnmatchedOutput()
        {
            #region Unmatched Output

            using (var matching = ProxyEntryPointFactory.GetMatchingEntryPoint(_serverName, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
            {

                /// If unmatched records exist
                if (_unmatchedRecordCount > 0)
                {

                    /// calculate the page size
                    var maxPageNumber = ((int)Math.Ceiling((float)_unmatchedRecordCount / (float)_chunkSize));
                    var pageSize = _chunkSize;
                    const bool isDescendingSorting = false;

                    const MatchingResultType resultType = MatchingResultType.Unmatched;
                    const bool allCollapsed = false;
                    var collapsedExpandedCluster = new Collection<long>();

                    var pageNumber = 0;
                    var resultsMatchedRecords = _chunkSize;

                    while (resultsMatchedRecords > 0)
                    {

                        /// Define the page and load it with the given datasource mapping id.
                        var unmatchedPageInfo = new MatchingResultsPageInfo(pageNumber, pageSize, null, isDescendingSorting, _minConfidence, resultType, allCollapsed, collapsedExpandedCluster);
                        var unmatchedResultsMatched = matching.MatchingResultsGetPage(unmatchedPageInfo, _dataSourceMapping.Id);

                        var listColumnNameAndTypes = unmatchedResultsMatched.DataColumnNamesAndTypes.ToList();

                        resultsMatchedRecords = unmatchedResultsMatched.Records.Count();

                        /// Write the records to the output buffer
                        foreach (var record in unmatchedResultsMatched.Records)
                        {

                            var i = 0;
                            _unmatchedOutputBuffer.AddRow();
                            
                            foreach (var values in record.DataValues)
                            {
                                try
                                {
                                    AddValueToBuffer(_unmatchedOutputBuffer, 
                                        GetOutputColumn(1, listColumnNameAndTypes[i].Name), 
                                        values, 
                                        GetOutputDataType(1, listColumnNameAndTypes[i].Name));
                                    i++;
                                }
                                catch (Exception ex)
                                {
                                    _unmatchedOutputBuffer[i] = null;
                                    InternalFireError(ex.ToString());
                                }
                            }
                        }

                        /// write a progress to thee log
                        InternalFireProgress("Unmatched records are written.", (100 / maxPageNumber * pageNumber));
                        pageNumber++;    
                    }
                    _unmatchedOutputBuffer.SetEndOfRowset();
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        //private void SurvivorshipOutput()
        //{
        //    #region Survivorship Output

        //    using (var matching = ProxyEntryPointFactory.GetMatchingEntryPoint(_serverName, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
        //    {

        //        if (_survivorshipRecordCount > 0)
        //        {

        //            const bool isDescendingSorting = false;
        //            const MatchingResultType resultType = MatchingResultType.Survivorship;

        //            const bool allCollapsed = false;

        //            var maxPageNumber = ((int)Math.Ceiling((float)_survivorshipRecordCount / (float)_chunkSize));
        //            var pageSize = _chunkSize;
        //            var collapsedExpandedCluster = new Collection<long>();

        //            for (var pageNumber = 0; pageNumber <= maxPageNumber; pageNumber++)
        //            {

        //                var survivorshipPageInfo = new MatchingResultsPageInfo(pageNumber, pageSize, null, isDescendingSorting, _minConfidence, resultType, allCollapsed, collapsedExpandedCluster);
        //                var survivorshipResultsMatched = matching.MatchingResultsGetPage(survivorshipPageInfo, _dataSourceMapping.Id);
        //                var listColumnNameAndTypes = survivorshipResultsMatched.DataColumnNamesAndTypes.ToList();

        //                foreach (var record in survivorshipResultsMatched.Records)
        //                {

        //                    var i = 0;
        //                    _survivorshipOutputBuffer.AddRow();

        //                    foreach (var values in record.DataValues)
        //                    {
        //                        try
        //                        {
        //                            AddValueToBuffer(_survivorshipOutputBuffer,
        //                                GetOutputColumn(1, listColumnNameAndTypes[i].Name),
        //                                values,
        //                                GetOutputDataType(1, listColumnNameAndTypes[i].Name));
        //                            i++;
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            _survivorshipOutputBuffer[i] = null;
        //                            InternalFireError(ex.ToString());
        //                        }
        //                    }
        //                }

        //                InternalFireProgress("Survivorship records are written.", (100 / maxPageNumber * pageNumber));

        //            }
        //            _survivorshipOutputBuffer.SetEndOfRowset();
        //        }
        //    }

        //    #endregion
        //}

        #endregion

        #region PostExecute

        /// <summary>
        /// Posts an Information message that all is done, and calls base...
        /// </summary>
        public override void PostExecute()
        {
            try
            {
                if (ComponentMetaData.CustomPropertyCollection["CleanUpDqsProjects"].Value.ToString() == "True") DeleteDqsProject();
            }
            catch 
            {
                InternalFireWarning("DQS project could not be deleted.");
            }
            ComponentUtility.CloseClientSession(ComponentMetaData, DatabaseName, _currentSession.SessionId);
            base.PostExecute();

        }

        #endregion

        #region SSIS Functions

        #region AcquireConnections

        /// <summary>
        /// AcquireConnections is called during both component design and execution. 
        /// </summary>
        /// <param name="transaction"></param>
        public override void AcquireConnections(object transaction)
        {
            if (ComponentMetaData.RuntimeConnectionCollection["DQSConnectionManager"].ConnectionManager != null)
            {
                _dqsConnection = null;
                if (ComponentMetaData.RuntimeConnectionCollection.Count > 0)
                {
                    if (ComponentMetaData.RuntimeConnectionCollection["DQSConnectionManager"].ConnectionManager != null)
                    {
                        /// Create a DQS connection
                        _dqsConnection = DtsConvert.GetWrapper(ComponentMetaData.RuntimeConnectionCollection["DQSConnectionManager"].ConnectionManager);
                    }
                }
            }

        }

        #endregion

        #region ReleaseConnections

        /// <summary>
        /// Called repeatedly during component design, and at the end of component execution.
        /// </summary>
        public override void ReleaseConnections()
        {
            _dqsConnection = null;
        }

        #endregion

        #region OnInputPathDetached

        /// <summary>
        /// This method is called when an IDTSPath100 object is deleted from the IDTSPathCollection100 collection. 
        /// </summary>
        /// <param name="inputId"></param>
        public override void OnInputPathDetached(int inputId)
        {
            base.OnInputPathDetached(inputId);
            ComponentMetaData.OutputCollection[0].OutputColumnCollection.RemoveAll();
            ComponentMetaData.OutputCollection[1].OutputColumnCollection.RemoveAll();
            //ComponentMetaData.OutputCollection[2].OutputColumnCollection.RemoveAll();
        }

        #endregion

        #region OnInputPathAttached

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputId"></param>
        public override void OnInputPathAttached(int inputId)
        {
            ReinitializeMetaData();
        }

        #endregion

        #region PerformUpgrade

        /// <summary>
        /// Base implementation of the PerformUpgrade method
        /// Will be supported in the next versions
        /// </summary>
        /// <param name="pipelineVersion"></param>
        public override void PerformUpgrade(int pipelineVersion)
        {
            var componentAttribute = (DtsPipelineComponentAttribute)Attribute.GetCustomAttribute(GetType(), typeof(DtsPipelineComponentAttribute), false);
            var runtimeVersion = componentAttribute.CurrentVersion;
            var metadataVersion = ComponentMetaData.Version;

            if (runtimeVersion != metadataVersion)
            {
                if (metadataVersion <= 0) { }
                ComponentMetaData.Version = runtimeVersion;
            }
        }

        #endregion

        #region InsertInput

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insertPlacement"></param>
        /// <param name="inputId"></param>
        /// <returns></returns>
        public override IDTSInput100 InsertInput(DTSInsertPlacement insertPlacement, int inputId)
        {
            throw new Exception("Component " + ComponentMetaData.Name + " does not allow adding inputs.");
        }

        #endregion

        #region InsertOutput

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insertPlacement"></param>
        /// <param name="outputId"></param>
        /// <returns></returns>
        public override IDTSOutput100 InsertOutput(DTSInsertPlacement insertPlacement, int outputId)
        {
            throw new Exception("Component " + ComponentMetaData.Name + " does not allow adding outputs.");
        }

        #endregion

        #endregion

        #region SSIS Helper Functions

        /// <summary>
        /// Create an external meta data columns
        /// </summary>
        /// <param name="externalCollection"></param>
        /// <param name="column"></param>
        private static void CreateExternalMetaDataColumn(IDTSExternalMetadataColumnCollection100 externalCollection, IDTSOutputColumn100 column)
        {
            // For each output column create an external meta data columns.
            IDTSExternalMetadataColumn100 eColumn = externalCollection.New();
            eColumn.Name = column.Name;
            eColumn.DataType = column.DataType;
            eColumn.Precision = column.Precision;
            eColumn.Length = column.Length;
            eColumn.Scale = column.Scale;

            // wire the output column to the external metadata
            column.ExternalMetadataColumnID = eColumn.ID;
        }

        /// <summary>
        ///  Writes the results of the last execution to the log
        /// </summary>
        private void WriteLastProcessSummary()
        {
            try
            {
                /// GetMatchingEntryPoint
                /// Get the MatchingResultsGetLastProcessSummary for the current project
                /// Send all information to the Log
                using (var matching = ProxyEntryPointFactory.GetMatchingEntryPoint(_serverName, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
                {
                    MatchingResultsSummaryStats summaryStats = matching.MatchingResultsGetLastProcessSummary();
                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "AverageClusterSize: {0}", summaryStats.AverageClusterSize.ToString(CultureInfo.InvariantCulture)));
                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "MaxClusterSize: {0}", summaryStats.MaxClusterSize));
                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "MinClusterSize: {0}", summaryStats.MinClusterSize));
                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "NumberOfClusters: {0}", summaryStats.NumberOfClusters));
                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "NumberOfSourceRecords: {0}", summaryStats.NumberOfSourceRecords));

                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "NumberOfDuplicateRecords: {0}", summaryStats.NumberOfDuplicateRecords));
                    _matchedRecordCount = summaryStats.NumberOfDuplicateRecords;

                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "NumberOfUnmatchedRecords: {0}", summaryStats.NumberOfUnmatchedRecords));
                    _unmatchedRecordCount = summaryStats.NumberOfUnmatchedRecords;

                    InternalFireInformation(String.Format(CultureInfo.InvariantCulture, "NumberOfClusters: {0}", summaryStats.NumberOfClusters));
                    _survivorshipRecordCount = summaryStats.NumberOfClusters;

                }
            }
            catch (Exception ex)
            {
                InternalFireWarning("The process summary could not be retrieved.");
                InternalFireWarning(ex.ToString());
            }
        }

        /// <summary>
        /// Creates a IDTSOutputColumn100 based on the Virtual Input Column
        /// </summary>
        /// <param name="output">The output for which the column should be created</param>
        /// <param name="virtualInputColumn">The VirtualInputColumn</param>
        /// <returns>Output Column</returns>
        private IDTSOutputColumn100 BuildColumn(IDTSOutput100 output, IDTSVirtualInputColumn100 virtualInputColumn)
        {
            return BuildColumn(output, virtualInputColumn.Name
                , virtualInputColumn.DataType
                , virtualInputColumn.Length
                , virtualInputColumn.Precision
                , virtualInputColumn.Scale
                , virtualInputColumn.CodePage
                , virtualInputColumn.Description);
        }

        /// <summary>
        /// Creates a IDTSOutputColumn100 based on the Virtual Input Column
        /// </summary>
        /// <param name="output">The output for which the column should be created</param>
        /// <param name="name">Column Name</param>
        /// <param name="dataType">Column Data Type</param>
        /// <param name="length">Column Length</param>
        /// <param name="precision">Column Precision</param>
        /// <param name="scale">Column Scale</param>
        /// <param name="codePage">Column CodePage</param>
        /// <param name="description">Column Description</param>
        /// <returns>Output Column</returns>
        private IDTSOutputColumn100 BuildColumn(IDTSOutput100 output, string name, DataType dataType, int length, int precision, int scale, int codePage, string description)
        {
            var column = output.OutputColumnCollection.New();
            column.Name = name;
            column.SetDataTypeProperties(dataType,
                                       length,
                                       precision,
                                       scale,
                                       codePage);
            column.Description = description;
            return column;
        }

        /// <summary>
        /// Write an Error message out to the Log.
        /// </summary>
        /// <param name="message">The error message to fire</param>
        private void InternalFireError(string message)
        {
            ComponentMetaData.FireError(0, ComponentMetaData.Name, message, string.Empty, 0, out _cancelEvent);
        }

        /// <summary>
        /// Write an information out to the Log.
        /// </summary>
        /// <param name="message">The information message to fire</param>
        private void InternalFireInformation(string message)
        {
            ComponentMetaData.FireInformation(0, ComponentMetaData.Name, message, string.Empty, 0, ref _cancelEvent);
        }

        /// <summary>
        /// Write a waring out to the log
        /// </summary>
        /// <param name="message">The warning message to fire</param>
        private void InternalFireWarning(string message)
        {
            ComponentMetaData.FireWarning(0, ComponentMetaData.Name, message, string.Empty, 0);
        }

        /// <summary>
        /// Write a porgress status out to the log
        /// </summary>
        /// <param name="message">The status message to fire</param>
        /// <param name="percentComplete">The progress in percent to fire</param>
        private void InternalFireProgress(string message, int percentComplete)
        {
            ComponentMetaData.FireProgress(message, percentComplete, 0, 100, ComponentMetaData.Name, ref _cancelEvent);
        }

        /// <summary>
        /// Returns the column ID based on the name
        /// </summary>
        /// <param name="bufferIndex"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private int GetOutputColumn(int bufferIndex, string columnName)
        {

            int retValue = 0;
            IDTSExternalMetadataColumnCollection100 exCol = ComponentMetaData.OutputCollection[bufferIndex].ExternalMetadataColumnCollection;           
            for (int i = 0; i < exCol.Count; i++)
            {
                if (exCol[i].Name == columnName)
                {
                    retValue = i;
                    break;
                }
            }
            return retValue;

        }

        /// <summary>
        /// Retunrs the data type of a specific column 
        /// </summary>
        /// <param name="bufferIndex"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private DataType GetOutputDataType(int bufferIndex, string columnName)
        {

            var exCol = ComponentMetaData.OutputCollection[bufferIndex].ExternalMetadataColumnCollection;
            for (var i = 0; i < exCol.Count; i++)
            {
                if (exCol[i].Name == columnName)
                {
                    return exCol[i].DataType;
                }
            }
            return DataType.DT_EMPTY;

        }

        /// <summary>
        /// Add a value to the output buffer
        /// </summary>
        /// <param name="outputBuffer">The output buffer</param>
        /// <param name="outputBufferIndex">The index defines the column</param>
        /// <param name="value">The value</param>
        /// <param name="dataType">The SISS DataTypefor the value</param>
        private static void AddValueToBuffer(PipelineBuffer outputBuffer, int outputBufferIndex, Object value, DataType dataType)
        {
            if (value == null || String.IsNullOrEmpty(value.ToString()))
            {
                outputBuffer.SetNull(outputBufferIndex);
            }
            else
            {
                switch (dataType)
                {
                    case DataType.DT_BOOL:
                        outputBuffer.SetBoolean(outputBufferIndex, Convert.ToBoolean(value));
                        break;
                    case DataType.DT_BYTES:
                        break;
                    case DataType.DT_CY:
                        outputBuffer.SetDecimal(outputBufferIndex, Convert.ToDecimal(value));
                        break;
                    case DataType.DT_DATE:
                        outputBuffer.SetDateTime(outputBufferIndex, Convert.ToDateTime(value));
                        break;
                    case DataType.DT_DBDATE:
                        //outputBuffer[outputBufferIndex] = value;
                        outputBuffer.SetDate(outputBufferIndex, Convert.ToDateTime(value));
                        break;
                    case DataType.DT_DBTIME:
                        break;
                    case DataType.DT_DBTIMESTAMP:
                        outputBuffer.SetDateTime(outputBufferIndex, Convert.ToDateTime(value));
                        break;
                    case DataType.DT_DECIMAL:
                        outputBuffer.SetDecimal(outputBufferIndex, Convert.ToDecimal(value));
                        break;
                    case DataType.DT_FILETIME:
                        outputBuffer[outputBufferIndex] = value;
                        break;
                    case DataType.DT_GUID:
                        outputBuffer.SetGuid(outputBufferIndex, new Guid(value.ToString()));
                        break;
                    case DataType.DT_I1:
                        outputBuffer.SetSByte(outputBufferIndex, Convert.ToSByte(value));
                        break;
                    case DataType.DT_I2:
                        outputBuffer.SetInt16(outputBufferIndex, Convert.ToInt16(value));
                        break;
                    case DataType.DT_I4:
                        outputBuffer.SetInt32(outputBufferIndex, Convert.ToInt32(value));
                        break;
                    case DataType.DT_I8:
                        outputBuffer.SetInt64(outputBufferIndex, Convert.ToInt64(value));
                        break;
                    case DataType.DT_IMAGE:
                        var colDtImage = (BlobColumn)value;
                        if (colDtImage.IsNull)
                        {
                            outputBuffer.SetNull(outputBufferIndex);
                        }
                        else
                        {
                            outputBuffer.AddBlobData(outputBufferIndex, colDtImage.GetBlobData(0, (int)colDtImage.Length));
                        }
                        break;
                    case DataType.DT_NTEXT:
                        var colDtNtext = (BlobColumn)value;
                        if (colDtNtext.IsNull)
                        {
                            outputBuffer.SetNull(outputBufferIndex);
                        }
                        else
                        {
                            outputBuffer.AddBlobData(outputBufferIndex, colDtNtext.GetBlobData(0, (int)colDtNtext.Length));
                        }
                        break;
                    case DataType.DT_NULL:
                        outputBuffer.SetNull(outputBufferIndex);
                        break;
                    case DataType.DT_NUMERIC:
                        outputBuffer.SetDecimal(outputBufferIndex, Convert.ToDecimal(value));
                        break;
                    case DataType.DT_R4:
                        outputBuffer.SetSingle(outputBufferIndex, Convert.ToSingle(value));
                        break;
                    case DataType.DT_R8:
                        outputBuffer.SetDouble(outputBufferIndex, Convert.ToDouble(value));
                        break;
                    case DataType.DT_STR:
                        outputBuffer.SetString(outputBufferIndex, value.ToString());
                        break;
                    case DataType.DT_TEXT:
                        outputBuffer.SetNull(outputBufferIndex);
                        break;
                    case DataType.DT_UI1:
                        outputBuffer.SetByte(outputBufferIndex, Convert.ToByte(value));
                        break;
                    case DataType.DT_UI2:
                        outputBuffer.SetUInt16(outputBufferIndex, Convert.ToUInt16(value));
                        break;
                    case DataType.DT_UI4:
                        outputBuffer.SetUInt32(outputBufferIndex, Convert.ToUInt32(value));
                        break;
                    case DataType.DT_UI8:
                        outputBuffer.SetUInt64(outputBufferIndex, Convert.ToUInt64(value));
                        break;
                    case DataType.DT_WSTR:
                        outputBuffer.SetString(outputBufferIndex, value.ToString());
                        break;
                    default:
                        outputBuffer.SetNull(outputBufferIndex);
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the column ID based on the name
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private int GetInputColumn(string columnName)
        {
            var retValue = 0;
            var virtualInput = ComponentMetaData.InputCollection[0].GetVirtualInput();
            
            for (var i = 0; i < virtualInput.VirtualInputColumnCollection.Count; i++)
            {
                if (virtualInput.VirtualInputColumnCollection[i].Name == columnName)
                {
                    retValue = i;
                    break;
                }
            }

            return retValue;
        }

        /// <summary>
        /// Returns the column DataType based on the column ID
        /// </summary>
        /// <param name="columnId"></param>
        /// <returns></returns>
        private DataType GetInputColumnDataType(int columnId)
        {
            var virtualInput = ComponentMetaData.InputCollection[0].GetVirtualInput();
            return virtualInput.VirtualInputColumnCollection[columnId].DataType;
        }

        #endregion

        #region DQS Functions

        /// <summary>
        /// 
        /// </summary>
        private void UpdateDqsProject()
        {
            using (var knowledgebaseManagementEntryPoint = ComponentUtility.GetKnowledgebaseManagementEntryPoint(ComponentMetaData, DatabaseName, _currentSession.SessionId))
            {
                _dqsProject = knowledgebaseManagementEntryPoint.DQProjectUpdate(_dqsProject);
            }

        }

        /// <summary>
        /// Loads the data records on the DQS Server
        /// </summary>
        /// <param name="bufferRecords">Records from the current buffer</param>
        /// <returns></returns>
        private bool UploadRecords(List<ReadOnlyStringCollection> bufferRecords)
        {

            bool status;
            try
            {
                /// GetMetadataManagementEntryPoint
                using (var metadata = ComponentUtility.GetMetadataManagementEntryPoint(_dqsConnection, DatabaseName, _currentSession.SessionId, _dqsProject.Id))
                {
                    if (_dataSourceMapping == null) _dataSourceMapping = metadata.DataSourceMappingCreate(GetDataSourceMapping());
                    var records = new ReadOnlyCollection<ReadOnlyStringCollection>(bufferRecords);
                    metadata.DataSourceMappingAddRecords(_dataSourceMapping.Id, records);
                }
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
                InternalFireError(ex.ToString());
            }

            return status;

        }

        /// <summary>
        /// Deletes the current DQS Project
        /// </summary>
        private void DeleteDqsProject()
        {
            KnowledgebaseManagementEntryPointClient knowledgebaseManagementEntryPoint;

            /// GetKnowledgebaseManagementEntryPoint 
            using (knowledgebaseManagementEntryPoint = ComponentUtility.GetKnowledgebaseManagementEntryPoint(ComponentMetaData, DatabaseName, _currentSession.SessionId))
            {
                if (knowledgebaseManagementEntryPoint.DQProjectGetById(_dqsProject.Id) != null) knowledgebaseManagementEntryPoint.DQProjectDelete(_dqsProject);
            }
        }

        /// <summary>
        /// Returns the DQS data type for a specified column
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private FieldType GetFieldType(string columnName)
        {
            FieldType retValue;

            var columnDataType = GetInputColumnDataType(GetInputColumn(columnName));
            switch (columnDataType)
            {

                case DataType.DT_BOOL:
                    retValue = FieldType.String;
                    break;
                case DataType.DT_BYTES:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_CY:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_DATE:
                    retValue = FieldType.Date;
                    break;
                case DataType.DT_DBDATE:
                    retValue = FieldType.Date;
                    break;
                case DataType.DT_DBTIME:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_DBTIME2:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_DBTIMESTAMP:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_DBTIMESTAMP2:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_DBTIMESTAMPOFFSET:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_DECIMAL:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_EMPTY:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_FILETIME:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_GUID:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_I1:
                    retValue = FieldType.Integer;
                    break;
                case DataType.DT_I2:
                    retValue = FieldType.Integer;
                    break;
                case DataType.DT_I4:
                    retValue = FieldType.Integer;
                    break;
                case DataType.DT_I8:
                    retValue = FieldType.Integer;
                    break;
                case DataType.DT_IMAGE:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_NTEXT:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_NULL:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_NUMERIC:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_R4:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_R8:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_STR:
                    retValue = FieldType.String;
                    break;
                case DataType.DT_TEXT:
                    retValue = FieldType.None;
                    break;
                case DataType.DT_UI1:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_UI2:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_UI4:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_UI8:
                    retValue = FieldType.Decimal;
                    break;
                case DataType.DT_WSTR:
                    retValue = FieldType.String;
                    break;
                default:
                    retValue = FieldType.None;
                    break;

            }

            return retValue;
        }

        /// <summary>
        /// Create the data source mapping
        /// </summary>
        /// <returns></returns>
        private DataSourceMapping GetDataSourceMapping()
        {
            
            /// create DataSourceMappingField collection
            var collFieldMappings = new Collection<DataSourceMappingField>();

            /// Column <=> Domain mapping
            string[] mapping = ComponentMetaData.CustomPropertyCollection["MappingValues"].Value.ToString().Split(';');

            var i = 0;

            /// iterate through all input columns and create a DataSourceMappingField
            foreach (var col in _columns)
            {
                
                /// checks if the column has a mapping
                var arrayRow = Array.Find(mapping, element => element.StartsWith(col.ToString() + "|"));
                string strDomain = null;

                if (!String.IsNullOrEmpty(arrayRow)) strDomain = arrayRow.Split('|')[1];              

                DataSourceMappingField field;

                /// if there is a mapping, domain and fieldFunctionalType are set
                if (!String.IsNullOrEmpty(strDomain))
                {
                    var fieldFunctionalType = FieldFunctionalType.Domain;
                    var domainId = GetDomainIdByName(strDomain, _knowledgeBaseName);
                    if (domainId == 0)
                    {
                        domainId = GetCompositeDomainIdByName(strDomain, _knowledgeBaseName);
                        fieldFunctionalType = FieldFunctionalType.CompositeDomain;
                    }

                    field = new DataSourceMappingField(col.ToString(), fieldFunctionalType, domainId);

                }
                else
                {
                    /// if ther isno mapping FieldFunctionalType = FieldNotMapped
                    field = new DataSourceMappingField(col.ToString(), FieldFunctionalType.FieldNotMapped, 0);
                }

                /// set the DataSourceFieldType
                field.DataSourceFieldType = GetFieldType(col.ToString());
                field.Id = i++;
                collFieldMappings.Add(field);

            }

            /// create the DataSourceMapping
            var objArray = new object[] { "Data Source Mapping", Guid.NewGuid() };
            var dataSourceMapping = new DataSourceMapping
            {
                Name = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", objArray),
                Description = "This schema is used by SSIS oh22is DQS Matching Transformation",
                MappingType = DataSourceMappingType.DeDuplicationDataSourceMapping,
                DataSourceType = DataSourceType.External,
                DataSourceMappingFields = collFieldMappings,
            };

            return dataSourceMapping;
        }

        /// <summary>
        /// check if the ConnectionManager is a DQS ConnectionManager
        /// </summary>
        /// <param name="cm"></param>
        /// <returns></returns>
        public bool IsDqsConnection(ConnectionManager cm)
        {
            return cm.CreationName.StartsWith("DQS");
        }

        /// <summary>
        /// GetKnowledgeBaseIdByName
        /// </summary>
        /// <param name="knowledgeBaseName">Name of the knowledge base</param>
        /// <returns></returns>
        private long GetKnowledgeBaseIdByName(string knowledgeBaseName)
        {
            long id = 0;

            /// GetKnowledgebaseManagementEntryPoint
            using (var knowledgeBase = ProxyEntryPointFactory.GetKnowledgebaseManagementEntryPoint(_serverName, DatabaseName, _currentSession.SessionId))
            {
                var allKbs = knowledgeBase.KnowledgebaseGet();
                foreach (var kb in allKbs.Where(kb => kb.Name == knowledgeBaseName))
                {
                    id = kb.Id;
                    break;
                }
            }

            return id;
        }

        /// <summary>
        /// GetDomainIdByName
        /// </summary>
        /// <param name="domainName">Name of the domain</param>
        /// <param name="knowledgeBaseName">Name of the knowledge base</param>
        /// <returns></returns>
        private long GetDomainIdByName(string domainName, string knowledgeBaseName)
        {
            long id = 0;
            try
            {
                if (knowledgeBaseName == null) throw new ArgumentNullException("knowledgeBaseName");
                using (var metadata = ProxyEntryPointFactory.GetMetadataManagementEntryPoint(_serverName, DatabaseName, _currentSession.SessionId, _knowledgeBaseId))
                {
                    var domain = metadata.DomainGetByName(domainName);
                    if (domain != null)
                    {
                        id = domain.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                InternalFireError(ex.ToString());
            }
            return id;
        }

        /// <summary>
        /// GetCompositeDomainIdByName
        /// </summary>
        /// <param name="domainName">Name of the domain</param>
        /// <param name="knowledgeBaseName">Name of the knowledge base</param>
        /// <returns></returns>
        private long GetCompositeDomainIdByName(string domainName, string knowledgeBaseName)
        {
            long id = 0;
            try
            {
                if (knowledgeBaseName == null) throw new ArgumentNullException("knowledgeBaseName");
                using (
                    var metadata = ProxyEntryPointFactory.GetMetadataManagementEntryPoint(_serverName, DatabaseName,
                        _currentSession.SessionId, _knowledgeBaseId))
                {
                    var domain = metadata.CompositeDomainGetByName(domainName.Replace(" (CD)", ""));
                    if (domain != null)
                    {
                        id = domain.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                InternalFireWarning(ex.ToString());
            }
            return id;
        }

        /// <summary>
        /// Creates the client session
        /// </summary>
        /// <returns></returns>
        private NotificationSessionInfo CreateClientSession()
        {
            // Get the connection string parameters
            var notificationEp = ProxyEntryPointFactory.GetNotificationEntryPoint(_serverName, DatabaseName);
            var clientSession = notificationEp.NotificationSessionCreate(ClientType.SsisClient);
            return clientSession;
        }

        #endregion

        #region Validations

        /// <summary>
        /// Validates the settings of the component
        /// </summary>
        /// <returns></returns>
        public override DTSValidationStatus Validate()
        {

            var dtsValidationStatus = ComponentValidation.ValidateComponentInitialData(ComponentMetaData, AdditionalColumns);
            
            if (dtsValidationStatus != DTSValidationStatus.VS_ISVALID)
            {
                return dtsValidationStatus;
            }

            if (ComponentMetaData.InputCollection.Count == 0) return DTSValidationStatus.VS_ISCORRUPT;
            if (ComponentMetaData.OutputCollection.Count == 0) return DTSValidationStatus.VS_ISCORRUPT;
          
            if (!InitializeProxy())
            {
                ComponentUtility.FireError(ComponentMetaData, ComponentMessage.CommonNoMappingsSpecified);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            //CommonNoMappingsSpecified
            if (ComponentMetaData.CustomPropertyCollection["MappingValues"].Value == null)
            {
                ComponentUtility.FireError(ComponentMetaData, ComponentMessage.CommonNoMappingsSpecified);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if (ComponentValidation.DoesInputColumnsMatchDomains(ComponentMetaData) == false)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Input column mapping is not set correctly.", "", 0, out _cancelEvent);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            int minConfidence = Convert.ToInt32(ComponentMetaData.CustomPropertyCollection["ScoreValue"].Value.ToString());
            if (minConfidence < 50 || minConfidence > 100)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "Minimal Matching Score is not set correctly.", "", 0, out _cancelEvent);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if (!ComponentValidation.DoesInputColumDataTypesAreValid(ComponentMetaData.InputCollection[0].GetVirtualInput()))
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "An invalid input column data type was specified.", "", 0, out _cancelEvent);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            #region DQS related validations

            if (ComponentMetaData.CustomPropertyCollection["DqsKnowledgeBase"].Value == null)
            {
                ComponentMetaData.FireError(0, ComponentMetaData.Name, "A knowledge base is not specified.", "", 0, out _cancelEvent);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            /// checks the DQS server settings
            try
            {
                if (_dqsConnection != null)
                {
                    VerificationResult verificationResult = ComponentUtility.VerifyServer(_dqsConnection.ConnectionString, DatabaseName);
                    if (verificationResult == VerificationResult.ServerVerified)
                    {

                        /// checks if the DQS communication is valid
                        if (!ComponentUtility.IsCommunicationValid(ComponentMetaData, DatabaseName))
                        {
                            return DTSValidationStatus.VS_ISBROKEN;
                        }

                    }
                    switch (verificationResult)
                    {

                        case VerificationResult.ClientNotCompatible:
                            {
                                ComponentUtility.FireError(ComponentMetaData, ComponentMessage.ServerVersionNotCompatible);
                                break;
                            }
                        case VerificationResult.FrameworkVersionNotCompatible:
                            {
                                object[] objArray = { "DqsInstaller.exe -upgrade" };
                                ComponentUtility.FireError(ComponentMetaData, ComponentMessage.ServerFrameworkNotCompatible, objArray);
                                break;
                            }
                    }
                }
                else
                {
                    ComponentUtility.FireError(ComponentMetaData, ComponentMessage.DataQualityConnectionManagerNoConnectionExist);
                    return DTSValidationStatus.VS_ISBROKEN;
                }
            }
            catch (Exception ex)
            {
                InternalFireError("An error occurred while retrieving the connection manager.");
                InternalFireError(ex.ToString());
                return DTSValidationStatus.VS_ISBROKEN;
            }

            #endregion

            if (!ComponentValidation.DoesExternalMetaDataMatchOutputMetaData(ComponentMetaData.OutputCollection[0])) return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            if (!ComponentValidation.DoesExternalMetaDataMatchOutputMetaData(ComponentMetaData.OutputCollection[1])) return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            //if (!ComponentValidation.DoesExternalMetaDataMatchOutputMetaData(base.ComponentMetaData.OutputCollection[2])) return DTSValidationStatus.VS_NEEDSNEWMETADATA;

            ComponentValidation.FixInvalidInputColumnMetaData(ComponentMetaData.InputCollection[0]);

            /// Finally, call the base class, which validates that the LineageID of each column in the input collection
            /// matches the LineageID of a column in the VirtualInputColumnCollection.
            return base.Validate();

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool InitializeProxy()
        {
            try
            {
                ProxyInit.UseEncryption = Convert.ToBoolean(ComponentMetaData.CustomPropertyCollection["EncryptConnection"].Value);
                if (!ProxyInit.IsInit)
                {
                    ProxyInit.Init(new SsisProxyInitParameters());
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}



