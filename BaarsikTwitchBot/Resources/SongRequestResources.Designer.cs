﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BaarsikTwitchBot.Resources {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SongRequestResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SongRequestResources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BaarsikTwitchBot.Resources.SongRequestResources", typeof(SongRequestResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Сейчас играет: &apos;{0}&apos; от @{1}.
        /// </summary>
        internal static string Announce_CurrentSong {
            get {
                return ResourceManager.GetString("Announce_CurrentSong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} забанил трек от @{2}.
        /// </summary>
        internal static string BanSongChatHook_Banned_NoSongName {
            get {
                return ResourceManager.GetString("BanSongChatHook_Banned_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} забанил трек &apos;{1}&apos; от @{2}.
        /// </summary>
        internal static string BanSongChatHook_Banned_SongName {
            get {
                return ResourceManager.GetString("BanSongChatHook_Banned_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} принудительно скипнул трек от @{2}.
        /// </summary>
        internal static string ForceSkipSongChatHook_Skipped_NoSongName {
            get {
                return ResourceManager.GetString("ForceSkipSongChatHook_Skipped_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} принудительно скипнул трек &apos;{1}&apos; от @{2}.
        /// </summary>
        internal static string ForceSkipSongChatHook_Skipped_SongName {
            get {
                return ResourceManager.GetString("ForceSkipSongChatHook_Skipped_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} перевел текущий трек в награду &apos;{2}&apos;.
        /// </summary>
        internal static string LimitSongChatHook_Success_NoSongName {
            get {
                return ResourceManager.GetString("LimitSongChatHook_Success_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} перевел трек &apos;{1}&apos; в награду &apos;{2}&apos;.
        /// </summary>
        internal static string LimitSongChatHook_Success_SongName {
            get {
                return ResourceManager.GetString("LimitSongChatHook_Success_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} приостановил трек от @{2}.
        /// </summary>
        internal static string PauseSongChatHook_Pause_NoSongName {
            get {
                return ResourceManager.GetString("PauseSongChatHook_Pause_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} приостановил трек &apos;{1}&apos; от @{2}.
        /// </summary>
        internal static string PauseSongChatHook_Pause_SongName {
            get {
                return ResourceManager.GetString("PauseSongChatHook_Pause_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} возобновил трек от @{2}.
        /// </summary>
        internal static string PauseSongChatHook_Resume_NoSongName {
            get {
                return ResourceManager.GetString("PauseSongChatHook_Resume_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} возобновил трек &apos;{1}&apos; от @{2}.
        /// </summary>
        internal static string PauseSongChatHook_Resume_SongName {
            get {
                return ResourceManager.GetString("PauseSongChatHook_Resume_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, видео длительностью более {1} ({2}).
        /// </summary>
        internal static string Reward_MaxDurationExceeded {
            get {
                return ResourceManager.GetString("Reward_MaxDurationExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, данная песня может быть заказана только наградой &apos;{2}&apos;.
        /// </summary>
        internal static string Reward_RequestInPlusOnly_NoSongName {
            get {
                return ResourceManager.GetString("Reward_RequestInPlusOnly_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, песня &apos;{1}&apos; может быть заказана только наградой &apos;{2}&apos;.
        /// </summary>
        internal static string Reward_RequestInPlusOnly_SongName {
            get {
                return ResourceManager.GetString("Reward_RequestInPlusOnly_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Добавлена песня от @{1} (#{2}).
        /// </summary>
        internal static string Reward_SongAdded_NoSongName {
            get {
                return ResourceManager.GetString("Reward_SongAdded_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Добавлена песня &apos;{0}&apos; от @{1} (#{2}).
        /// </summary>
        internal static string Reward_SongAdded_SongName {
            get {
                return ResourceManager.GetString("Reward_SongAdded_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, данная песня уже находится в очереди.
        /// </summary>
        internal static string Reward_SongInQueue_NoSongName {
            get {
                return ResourceManager.GetString("Reward_SongInQueue_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, песня &apos;{1}&apos; уже находится в очереди.
        /// </summary>
        internal static string Reward_SongInQueue_SongName {
            get {
                return ResourceManager.GetString("Reward_SongInQueue_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, данная песня запрещена к заказу стримером.
        /// </summary>
        internal static string Reward_SongIsBanned {
            get {
                return ResourceManager.GetString("Reward_SongIsBanned", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, стрим не поддерживается.
        /// </summary>
        internal static string Reward_StreamNotSupported {
            get {
                return ResourceManager.GetString("Reward_StreamNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на @{0}, видео по данному заказу не найдено.
        /// </summary>
        internal static string Reward_VideoNotFound {
            get {
                return ResourceManager.GetString("Reward_VideoNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Трек &apos;{0}&apos; нельзя скипнуть командой !skip.
        /// </summary>
        internal static string SkipSongChatHook_NonSkippable {
            get {
                return ResourceManager.GetString("SkipSongChatHook_NonSkippable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} скипнул трек от @{2}.
        /// </summary>
        internal static string SkipSongChatHook_Skipped_NoSongName {
            get {
                return ResourceManager.GetString("SkipSongChatHook_Skipped_NoSongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {0} скипнул трек &apos;{1}&apos; от @{2}.
        /// </summary>
        internal static string SkipSongChatHook_Skipped_SongName {
            get {
                return ResourceManager.GetString("SkipSongChatHook_Skipped_SongName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на На данный момент нет заказанных треков.
        /// </summary>
        internal static string SongNameChatHook_NoRequests {
            get {
                return ResourceManager.GetString("SongNameChatHook_NoRequests", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на В текущий момент музыка на паузе.
        /// </summary>
        internal static string SongNameChatHook_Paused {
            get {
                return ResourceManager.GetString("SongNameChatHook_Paused", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Играет трек &apos;{0}&apos; от @{1}.
        /// </summary>
        internal static string SongNameChatHook_Playing {
            get {
                return ResourceManager.GetString("SongNameChatHook_Playing", resourceCulture);
            }
        }
    }
}