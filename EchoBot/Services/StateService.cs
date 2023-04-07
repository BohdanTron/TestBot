using EchoBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace EchoBot.Services
{
    public class StateService
    {
        public UserState UserState { get; }
        public ConversationState ConversationState { get; }

        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }
        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }


        public StateService(UserState userState, ConversationState conversationState)
        {
            UserState = userState;
            ConversationState = conversationState;

            InitializeAccessors();
        }

        public void InitializeAccessors()
        {
            UserProfileAccessor = UserState.CreateProperty<UserProfile>($"{nameof(StateService)}.{nameof(UserProfile)}");

            ConversationDataAccessor = ConversationState.CreateProperty<ConversationData>($"{nameof(StateService)}.{nameof(ConversationData)}");
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>($"{nameof(StateService)}.{nameof(DialogState)}");
        }
    }
}
