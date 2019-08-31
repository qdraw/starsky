import { IArchive } from './IArchive';
import { IDetailView } from './IDetailView';

/**
 * In plaats van een type met alleen strings defineren we hier een
 * type met zowel een key als een "Value" (voor zover types values hebben)
 */
// let op, hij maakt hier stiekem een interface van ipv type
export interface IMediaTypes {
  DetailView: IDetailView,
  Archive: IArchive,
}

export interface IMedia<T extends keyof IMediaTypes = keyof IMediaTypes> {
  type: T;
  data: IMediaTypes[T];
}

export function newIMedia(): IMedia {
  return {} as IMedia;
}

// // import { ChatServiceConfig } from './chat-service-config.types';
// // import { ChatService } from './chat-service.types';
// // import { LivePresenceChatService } from './live-presence-chat-service';
// // import { SocketChatService } from './socket-chat-service';

// export enum IMediaTypes {
//   DetailView,
//   Archive,
// }

// /**
//  * Deze gebruiken we voor onze object mapper. De input types (enumLetterTypes)
//  * worden hier gebruikt als KEY en dienen een matchende value te hebben van het
//  * type { new(): Alphabet }. Oftewel: een class die Alphabet implementeerd.
//  */
// type chatEnumMapper = {
//   [key in keyof typeof IMediaTypes]: { new(config: Partial<ChatServiceConfig>): ChatService };
// };

// /**
//  * Dit is onze daadwerkelijke mapper constant. Hierin matchen we de enumLetterTypes
//  * met daadwerkelijke classes
//  */
// const chatMapper: chatEnumMapper = {
//   DetailView: IDetailView,
//   LivePresence: LivePresenceChatService,
//   Genesys: LivePresenceChatService,
// };

// /**
//  * En als laatste de Factory. Het eerste argument moet een key zijn van onze mapper constant.
//  * De return type is een nieuwe instantie van de class welke Alphabet implementeerd.
//  */
// export const ChatFactory = (type: keyof chatEnumMapper, config: Partial<ChatServiceConfig> = {}): ChatService => {
//   return new chatMapper[type](config);
// };
