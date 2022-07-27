using System;
using System.Linq;
using Microsoft.Ssdqs.Component.Common.Utilities;
using Microsoft.Ssdqs.Component.Common.Messages;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;

namespace oh22is.SqlServer.DQS
{
    /// <summary>
    /// Helper class for validating the columns in input and output objects. 
    /// </summary>
    public class ComponentValidation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool DoesInputColumnMatchVirtualInputColumns(IDTSInput100 input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var vInput = input.GetVirtualInput();
            var areAllColumnsValid = true;

            //	Verify that the columns in the input, have the same column metadata 
            // as the matching virtual input column.
            foreach (IDTSInputColumn100 column in input.InputColumnCollection)
            {
                //	Get the upstream column.
                var vColumn = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(column.LineageID);
                if (!DoesColumnMetaDataMatch(column, vColumn))
                {
                    areAllColumnsValid = false;
                    bool cancel;
                    input.Component.FireError(
                        0,
                        input.Component.Name,
                        @"The input column metadata for column" + column.IdentificationString + @" does not match its upstream column.",
                        @"",
                        0,
                        out cancel);
                }
            }

            return areAllColumnsValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="vColumn"></param>
        /// <returns></returns>
        private static bool DoesColumnMetaDataMatch(IDTSInputColumn100 column, IDTSVirtualInputColumn100 vColumn)
        {
            if (vColumn.DataType == column.DataType
                && vColumn.Precision == column.Precision
                && vColumn.Length == column.Length
                && vColumn.Scale == column.Scale)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        public static void FixInvalidInputColumnMetaData(IDTSInput100 input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            IDTSVirtualInput100 vInput = input.GetVirtualInput();

            foreach (IDTSInputColumn100 inputColumn in input.InputColumnCollection)
            {
                IDTSVirtualInputColumn100 vColumn = vInput.VirtualInputColumnCollection.GetVirtualInputColumnByLineageID(inputColumn.LineageID);

                if (!DoesColumnMetaDataMatch(inputColumn, vColumn))
                {
                    vInput.SetUsageType(vColumn.LineageID, inputColumn.UsageType);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool DoesOutputColumnMetaDataMatchExternalColumnMetaData(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            bool areAllOutputColumnsValid = true;

            if (output.ExternalMetadataColumnCollection.Count == 0)
            {
                return false;
            }

            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                IDTSExternalMetadataColumn100 exColumn
                    = output.ExternalMetadataColumnCollection.GetObjectByID(
                    column.ExternalMetadataColumnID);

                if (!DoesColumnMetaDataMatch(column, exColumn))
                {
                    bool cancel;
                    output.Component.FireError(
                        0,
                        output.Component.Name,
                        @"The output column " + column.IdentificationString + @" does not match the external metadata.",
                        @"",
                        0,
                        out cancel);
                    areAllOutputColumnsValid = false;
                }
            }
            return areAllOutputColumnsValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool DoesExternalMetaDataMatchOutputMetaData(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            IDTSExternalMetadataColumnCollection100 externalMetaData = output.ExternalMetadataColumnCollection;
            return output.OutputColumnCollection.Cast<IDTSOutputColumn100>().All(column => DoesColumnMetaDataMatch(column, externalMetaData.GetObjectByID(column.ExternalMetadataColumnID)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        public static void FixExternalMetaDataColumns(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            IDTSExternalMetadataColumnCollection100 externalMetaData = output.ExternalMetadataColumnCollection;
            externalMetaData.RemoveAll();

            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                IDTSExternalMetadataColumn100 exColumn = externalMetaData.New();
                exColumn.Name = column.Name;
                exColumn.DataType = column.DataType;
                exColumn.Precision = column.Precision;
                exColumn.Scale = column.Scale;
                exColumn.Length = column.Length;

                column.ExternalMetadataColumnID = exColumn.ID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="exCol"></param>
        /// <returns></returns>
        private static bool DoesColumnMetaDataMatch(IDTSOutputColumn100 column, IDTSExternalMetadataColumn100 exCol)
        {
            if (column.DataType == exCol.DataType
                && column.Precision == exCol.Precision
                && column.Length == exCol.Length
                && column.Scale == exCol.Scale)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        public static void FixOutputColumnMetaData(IDTSOutput100 output)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            if (output.ExternalMetadataColumnCollection.Count == 0)
            {
                return;
            }

            foreach (IDTSOutputColumn100 column in output.OutputColumnCollection)
            {
                IDTSExternalMetadataColumn100 exColumn
                    = output.ExternalMetadataColumnCollection.GetObjectByID(
                    column.ExternalMetadataColumnID);

                if (!DoesColumnMetaDataMatch(column, exColumn))
                {
                    column.SetDataTypeProperties(
                        exColumn.DataType,
                        exColumn.Length,
                        exColumn.Precision,
                        exColumn.Scale,
                        exColumn.CodePage);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentMetaData"></param>
        /// <param name="additionalColumns"></param>
        /// <returns></returns>
        public static DTSValidationStatus ValidateComponentInitialData(IDTSComponentMetaData100 componentMetaData, int additionalColumns)
        {

            var dtsInput = componentMetaData.InputCollection[0];
            var virtualInput = dtsInput.GetVirtualInput();
            var matchedOutput = componentMetaData.OutputCollection[0];
            var unmatchedOutput = componentMetaData.OutputCollection[1];
            //var survivorshipOutput = componentMetaData.OutputCollection[2];

            if (componentMetaData.InputCollection.Count != 1)
            {
                ComponentUtility.FireError(componentMetaData, ComponentMessage.DataCorrectionWrongNumberOfInputs);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (componentMetaData.OutputCollection.Count != 2)
            {
                ComponentUtility.FireError(componentMetaData, ComponentMessage.DataCorrectionWrongNumberOfOutputs);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (componentMetaData.OutputCollection[0].SynchronousInputID != 0)
            {
                ComponentUtility.FireError(componentMetaData, ComponentMessage.DataCorrectionOutputMustBeAsynchronous);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (componentMetaData.OutputCollection[1].SynchronousInputID != 0)
            {
                ComponentUtility.FireError(componentMetaData, ComponentMessage.DataCorrectionOutputMustBeAsynchronous);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            //if (componentMetaData.OutputCollection[2].SynchronousInputID != 0)
            //{
            //    ComponentUtility.FireError(componentMetaData, ComponentMessage.DataCorrectionOutputMustBeAsynchronous);
            //    return DTSValidationStatus.VS_ISCORRUPT;
            //}

            if (componentMetaData.OutputCollection[0].IsErrorOut || componentMetaData.OutputCollection[1].IsErrorOut)
            {
                ComponentUtility.FireError(componentMetaData, ComponentMessage.DataCorrectionOutputMarkedAsErrorOutput);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            if (!dtsInput.IsAttached)
            {
                ComponentUtility.FireError(componentMetaData, ComponentMessage.CommonNeedInputStream);
                return DTSValidationStatus.VS_ISBROKEN;
            }

            if (virtualInput.VirtualInputColumnCollection.Count + additionalColumns != matchedOutput.OutputColumnCollection.Count)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            if (virtualInput.VirtualInputColumnCollection.Count != unmatchedOutput.OutputColumnCollection.Count)
            {
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            //if (virtualInput.VirtualInputColumnCollection.Count != survivorshipOutput.OutputColumnCollection.Count)
            //{
            //    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            //}

            foreach (IDTSVirtualInputColumn100 inputColumn in virtualInput.VirtualInputColumnCollection)
            {

                try
                {
                    var column = matchedOutput.OutputColumnCollection[inputColumn.Name];
                    if (column != null)
                    {
                        if (column.DataType != inputColumn.DataType || column.Length != inputColumn.Length || column.Precision != inputColumn.Precision || column.Scale != inputColumn.Scale || column.CodePage != inputColumn.CodePage)
                        {
                            return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                        }
                    }
                }
                catch
                {
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }
            }

            return DTSValidationStatus.VS_ISVALID;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentMetaData"></param>
        /// <returns></returns>
        public static bool DoesInputColumnsMatchDomains(IDTSComponentMetaData100 componentMetaData)
        {
            const bool retValue = true;
            string[] mapping = componentMetaData.CustomPropertyCollection["MappingValues"].Value.ToString().Split(';');
            var virtualInput = componentMetaData.InputCollection[0].GetVirtualInput();

            for (var i = 0; i < mapping.Count(); i++)
            {
                try
                {
                    var vCol = virtualInput.VirtualInputColumnCollection.GetVirtualInputColumnByName("", mapping[i].Split('|')[0]);
                    if (vCol == null) return false;
                }
                catch
                {
                    return false;
                }
            }

            return retValue;
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool DoesInputColumDataTypesAreValid(IDTSVirtualInput100 virtualInput)
        {
            return virtualInput.VirtualInputColumnCollection.Cast<IDTSVirtualInputColumn100>().All(vCol => vCol.DataType != DataType.DT_IMAGE && vCol.DataType != DataType.DT_BYTES && vCol.DataType != DataType.DT_NTEXT && vCol.DataType != DataType.DT_FILETIME && vCol.DataType != DataType.DT_TEXT);
        }
    }
}
