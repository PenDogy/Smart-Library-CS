using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

[DataContract]
public class AddBookReqObj
{
    [DataMember]
    public string Title { get; set; }

    [DataMember]
    public string Author { get; set; }

    [DataMember]
    public int Cat_Id { get; set; }

    [DataMember]
    public string Shelf_location { get; set; }

    [DataMember]
    public int Copies { get; set; }
}