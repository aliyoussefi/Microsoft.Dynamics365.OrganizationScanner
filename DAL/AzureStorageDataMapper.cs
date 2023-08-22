using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Dynamics365.OrganizationScanner.DTO.SolutionHistoryDTO;

namespace Microsoft.Dynamics365.OrganizationScanner.DAL
{
    internal static class AzureStorageDataMapper
    {
        public static CloudBlobStream MapSolutionHistory(List<SolutionHistory> solutionHistories, CloudBlobStream blockBlob)
        {

                foreach (var rec in solutionHistories)
                {
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_endtime + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_errorcode + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_exceptionmessage + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_exceptionstack + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_ismanaged + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_ispatch + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_maxretries + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_name + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_operation + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_packagename + ","));


                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_packageversion + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_publisherid + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_publishername + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_result + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_retrycount + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionhistoryid + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionid + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_solutionversion + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_starttime + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_status + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_suboperation + ","));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes(rec.msdyn_totaltime + ""));
                    blockBlob.Write(System.Text.Encoding.Default.GetBytes("\n"));
                }
                return blockBlob;
        }
    }
}
