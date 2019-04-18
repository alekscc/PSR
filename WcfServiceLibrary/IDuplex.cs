using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract(SessionMode=SessionMode.Required,CallbackContract=typeof(IDuplexCallback))]
    public interface IDuplex
    {
        [OperationContract]
        ClientData Join(string name);
        [OperationContract(IsOneWay =true)]
        void TestService();
        [OperationContract(IsOneWay = true)]
        void SendResult(ClientData clientData);
        [OperationContract(IsOneWay = false)]
        int[][] GetMatrixData();
        [OperationContract(IsOneWay = false)]
        void SetStage(STAGE_TYPE type);


    }
    public interface IDuplexCallback
    {
        [OperationContract]
        int Test();
        [OperationContract(IsOneWay =true)]
        void Message(string content);
        [OperationContract(IsOneWay =true)]
        void SendData(ClientData clientData);
        [OperationContract]
        int DataSync(int[][] matrix);
        [OperationContract(IsOneWay = true)]
        void StartWork();
        [OperationContract(IsOneWay = true)]
        void JoinAccept();

    }

}
