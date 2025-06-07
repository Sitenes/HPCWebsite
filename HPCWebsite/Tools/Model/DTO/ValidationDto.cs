using Newtonsoft.Json.Linq;

namespace FrameWork.Model.DTO
{
    //public class CustomException<T>
    //{
    //    public CustomException(bool isSuccess, string parent, string messageName, T? value)
    //    {
    //        this.IsSuccess = isSuccess;
    //        this.MessageName = messageName;
    //        this.Parent = parent;
    //        Value = value;
    //    }


    //    public bool IsSuccess;
    //    public string Parent;
    //    public string MessageName;
    //    public T? Value;

    //    public string GetMessage(int statusId)
    //    {
    //        if (statusId >= 200 && statusId < 300)
    //            return GetSuccessMessage();
    //        else if (statusId >= 300 && statusId < 400)
    //            return GetWarnningMessage();
    //        else if (statusId >= 400 && statusId < 500)
    //            return GetWarnningMessage();
    //        else
    //            return GetErrorMessage();
    //    }
    //    private string GetErrorMessage()
    //    {
    //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Message.json");
    //        var jsonData = System.IO.File.ReadAllText(filePath);
    //        var jsonObject = JObject.Parse(jsonData);
    //        return (string)jsonObject["fa"]["Error"][this.Parent][this.MessageName];
    //    }
    //    private string GetWarnningMessage()
    //    {
    //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Message.json");
    //        var jsonData = System.IO.File.ReadAllText(filePath);
    //        var jsonObject = JObject.Parse(jsonData);
    //        return (string)jsonObject["fa"]["Warning"][this.Parent][this.MessageName];
    //    }
    //    private string GetSuccessMessage()
    //    {
    //        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Message.json");
    //        var jsonData = System.IO.File.ReadAllText(filePath);
    //        var jsonObject = JObject.Parse(jsonData);
    //        return (string)jsonObject["fa"]["Success"][this.Parent][this.MessageName];
    //    }
    //}
}
