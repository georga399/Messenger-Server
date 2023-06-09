using AutoMapper;
using Messenger.Models;
using Messenger.ViewModels;

namespace Messenger.Mappings;
public class ChatProfile: Profile
{
    public ChatProfile()
    {
        CreateMap<Chat, ChatViewModel>()
        .ForMember(dst => dst.Id, opt => opt.MapFrom(x=>x.Id))
        .ForMember(dst => dst.Title, opt => opt.MapFrom(x=>x.Title))
        .ForMember(dst => dst.IsGroup, opt => opt.MapFrom(x=>x.IsGroup))
        .ForMember(dst => dst.UsersId, opt => opt.MapFrom(x=>(from t in x.ChatUsers where true select t.UserId).ToList()))
        .ForMember(dst => dst.AdminId, opt => opt.MapFrom(x => x.AdminId))
        .ForMember(dst => dst.Avatar, opt => opt.MapFrom(x => x.Avatar));
        CreateMap<ChatViewModel, Chat>();
    }
}