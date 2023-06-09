using AutoMapper;
using Messenger.Models;
using Messenger.ViewModels;

namespace Messenger.Mappings;
public class MessageProfile: Profile
{
    public MessageProfile()
    {
        CreateMap<Message, MessageViewModel>()
        .ForMember(dst => dst.ChatId, opt => opt.MapFrom(x=>x.ChatId))
        .ForMember(dst => dst.Content, opt => opt.MapFrom(x=>x.Content))
        .ForMember(dst => dst.FromUserId, opt => opt.MapFrom(x=>x.FromUserId))
        .ForMember(dst => dst.FromUserName, opt => opt.MapFrom(x=>(x.FromUser == null) ? null : x.FromUser.UserName))
        .ForMember(dst => dst.Timestamp, opt => opt.MapFrom(x => x.Timestamp))
        .ForMember(dst => dst.Id, opt => opt.MapFrom(x => x.Id))
        .ForMember(dst => dst.AttachUri, opt => opt.MapFrom( x => x.AttachUri))
        .ForMember(dst => dst.FromUserAvaUri, opt => opt.MapFrom(x => x.FromUser!.Avatar));
        CreateMap<MessageViewModel, Message>();
    }
}