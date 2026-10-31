using System.Runtime.Serialization;

[DataContract]
public class AddMemberReqObj
{
    [DataMember]
    public string Fname { get; set; }

    [DataMember]
    public string Lname { get; set; }

    [DataMember]
    public int Mt_Id { get; set; }
}