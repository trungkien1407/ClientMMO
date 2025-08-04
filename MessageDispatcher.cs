using Assets.Script;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MessageDispatcher
{
    private Dictionary<string, Action<JObject>> _handlers = new();

    public void RegisterHandler(string action, Action<JObject> handler)
    {
        _handlers[action] = handler;
    }

    public void Dispatch(string message)
    {
        BaseMessage msg;

        try
        {
            msg = JsonConvert.DeserializeObject<BaseMessage>(message);
        }
        catch (Exception ex)
        {
            Debug.LogError(" Lỗi khi parse JSON: " + ex.Message);
            return;
        }

        if (msg != null && _handlers.TryGetValue(msg.Action, out var handler))
        {
            handler(msg.Data);
        }
        else
        {
            Debug.LogWarning($" Không có handler cho action: {msg?.Action}");
        }
    }
}