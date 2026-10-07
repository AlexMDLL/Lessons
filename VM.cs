using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp26.Data;
using WpfApp26.Models;

namespace WpfApp26
{
    // ViewModel главного окна
    internal class VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void N([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        readonly DispatcherTimer clock;
        readonly DateTime startedAt = DateTime.Now;

        NavItem selectedNavItem;
        string missionTime;
        string operationStatus;
        bool isDbAvailable;
        string connectionInfo;

        List<CrewMember> allCrew;
        string crewSearch;
        CrewMember selectedCrewMember;
        string damageText;
        string healText;
        string statusText;
        string crewLog;
        CancellationTokenSource searchCts;

        bool isLoadingMessages;
        bool isPinging;
        string pingResult;
        string serverTimeResult;
        string signalQuality;

        bool isDiagnosticsRunning;
        int crewProgress;
        int systemsProgress;
        int communicationProgress;
        int totalProgress;
        string verdict;
        string verdictDetail;
        bool isEmergency;

        public ObservableCollection<NavItem> NavItems { get; private set; }

        public NavItem SelectedNavItem
        {
            get { return selectedNavItem; }
            set
            {
                selectedNavItem = value;
                N();
                N(nameof(CurrentSection));
            }
        }

        public SectionType CurrentSection
        {
            get { return selectedNavItem == null ? SectionType.Crew : selectedNavItem.Section; }
        }

        public string MissionTime
        {
            get { return missionTime; }
            set { missionTime = value; N(); }
        }

        public string OperationStatus
        {
            get { return operationStatus; }
            set { operationStatus = value; N(); }
        }

        // true — данные читаются из PostgreSQL, false — показан демонстрационный набор
        public bool IsDbAvailable
        {
            get { return isDbAvailable; }
            set { isDbAvailable = value; N(); }
        }

        public string ConnectionInfo
        {
            get { return connectionInfo; }
            set { connectionInfo = value; N(); }
        }

        public bool IsEmergency
        {
            get { return isEmergency; }
            set
            {
                isEmergency = value;
                N();
                N(nameof(StationMode));
                N(nameof(StationModeText));
            }
        }

        public string StationMode
        {
            get { return isEmergency ? "АВАРИЯ" : "НОРМА"; }
        }

        public string StationModeText
        {
            get { return isEmergency ? "АВАРИЙНЫЙ РЕЖИМ" : "Штатный режим"; }
        }

        public int CrewOnBoard
        {
            get { return allCrew == null ? 0 : allCrew.Count; }
        }

        public ObservableCollection<CrewMember> Crew { get; private set; }

        public string CrewSearch
        {
            get { return crewSearch; }
            set
            {
                crewSearch = value;
                N();
                StartCrewSearch();
            }
        }

        public CrewMember SelectedCrewMember
        {
            get { return selectedCrewMember; }
            set
            {
                selectedCrewMember = value;
                N();
                N(nameof(HasSelectedCrewMember));
            }
        }

        public bool HasSelectedCrewMember
        {
            get { return selectedCrewMember != null; }
        }

        public string DamageText
        {
            get { return damageText; }
            set { damageText = value; N(); }
        }

        public string HealText
        {
            get { return healText; }
            set { healText = value; N(); }
        }

        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; N(); }
        }

        public string CrewLog
        {
            get { return crewLog; }
            set { crewLog = value; N(); }
        }

        public ObservableCollection<StationSystem> Systems { get; private set; }

        public int SystemsOkCount
        {
            get { return Systems == null ? 0 : Systems.Count(s => s.Status == "OK"); }
        }

        public int SystemsProblemCount
        {
            get { return Systems == null ? 0 : Systems.Count(s => s.Status != "OK"); }
        }

        public bool IsPinging
        {
            get { return isPinging; }
            set { isPinging = value; N(); N(nameof(IsNotPinging)); }
        }

        public bool IsNotPinging
        {
            get { return !isPinging; }
        }

        public string PingResult
        {
            get { return pingResult; }
            set { pingResult = value; N(); }
        }

        public string ServerTimeResult
        {
            get { return serverTimeResult; }
            set { serverTimeResult = value; N(); }
        }

        public string SignalQuality
        {
            get { return signalQuality; }
            set { signalQuality = value; N(); }
        }

        public ObservableCollection<string> PingHistory { get; private set; }

        public ObservableCollection<StationMessage> Messages { get; private set; }

        public bool IsLoadingMessages
        {
            get { return isLoadingMessages; }
            set { isLoadingMessages = value; N(); }
        }

        public bool IsDiagnosticsRunning
        {
            get { return isDiagnosticsRunning; }
            set { isDiagnosticsRunning = value; N(); N(nameof(IsNotDiagnosticsRunning)); }
        }

        public bool IsNotDiagnosticsRunning
        {
            get { return !isDiagnosticsRunning; }
        }

        public int CrewProgress
        {
            get { return crewProgress; }
            set { crewProgress = value; N(); }
        }

        public int SystemsProgress
        {
            get { return systemsProgress; }
            set { systemsProgress = value; N(); }
        }

        public int CommunicationProgress
        {
            get { return communicationProgress; }
            set { communicationProgress = value; N(); }
        }

        public int TotalProgress
        {
            get { return totalProgress; }
            set { totalProgress = value; N(); }
        }

        public string Verdict
        {
            get { return verdict; }
            set { verdict = value; N(); }
        }

        public string VerdictDetail
        {
            get { return verdictDetail; }
            set { verdictDetail = value; N(); }
        }

        public ObservableCollection<DiagnosticLine> DiagnosticResults { get; private set; }

        public BC RefreshCrewCommand
        {
            get
            {
                return new BC(async () =>
                {
                    CrewLog = "Запрос списка экипажа...";

                    try
                    {
                        // база пробуется каждый раз, перезапуск не нужен
                        await ReloadCrewAsync();
                        SetConnectionOk();
                        CrewLog = "Список экипажа получен из базы: " + allCrew.Count + " чел.";
                        OperationStatus = "Экипаж обновлён из базы в " + DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        await Task.Delay(250);
                        LoadCrewDemo();
                        CrewLog = string.Format("Демонстрационный набор: {0} чел.{1}{2}",
                            allCrew.Count, Environment.NewLine, Db.Hint(ex));
                        OperationStatus = "Работа в демонстрационном режиме";
                    }
                });
            }
        }

        public BC DamageCommand
        {
            get
            {
                return new BC(async () =>
                {
                    CrewMember member = SelectedCrewMember;
                    if (member == null)
                    {
                        CrewLog = "Сначала выберите члена экипажа в списке.";
                        return;
                    }

                    int damage = ParseAmount(DamageText);
                    if (damage <= 0)
                    {
                        CrewLog = "Введите целое число больше нуля в поле «Урон».";
                        return;
                    }

                    int before = member.Health;
                    int expected = Math.Max(before - damage, 0);
                    string shortName = member.ShortName;

                    try
                    {
                        // UPDATE crew SET health = GREATEST(health - @damage, 0) WHERE id = @id
                        await StationRepository.DamageAsync(member.Id, damage);
                        await ReloadCrewAsync();
                        SetConnectionOk();

                        int after = ReadHealth(member.Id, expected);

                        CrewLog = string.Format("{0}: нанесён урон {1}{2}{3} → {4}%{2}Операция выполнена.",
                            shortName, damage, Environment.NewLine, before, after);

                        AddMessage("Экипаж", after <= 40 ? "Критично" : "Инфо",
                            string.Format("{0}: здоровье {1}% → {2}%", shortName, before, after));

                        OperationStatus = "Нанесён урон " + shortName + " (" + damage + ")";
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        member.Health = expected;
                        CrewLog = string.Format("БД недоступна: {0}{1}{2}: изменение показано только на экране: {3} → {4}%.",
                            ShortMessage(ex), Environment.NewLine, shortName, before, member.Health);
                    }
                });
            }
        }

        public BC HealCommand
        {
            get
            {
                return new BC(async () =>
                {
                    CrewMember member = SelectedCrewMember;
                    if (member == null)
                    {
                        CrewLog = "Сначала выберите члена экипажа в списке.";
                        return;
                    }

                    int heal = ParseAmount(HealText);
                    if (heal <= 0)
                    {
                        CrewLog = "Введите целое число больше нуля в поле «Восстановление».";
                        return;
                    }

                    int before = member.Health;
                    int expected = Math.Min(before + heal, 100);
                    string shortName = member.ShortName;

                    try
                    {
                        // UPDATE crew SET health = LEAST(health + @heal, 100) WHERE id = @id
                        await StationRepository.HealAsync(member.Id, heal);
                        await ReloadCrewAsync();
                        SetConnectionOk();

                        int after = ReadHealth(member.Id, expected);

                        CrewLog = string.Format("{0}: восстановлено {1}{2}{3} → {4}%{2}Операция выполнена.",
                            shortName, heal, Environment.NewLine, before, after);

                        AddMessage("Медицина", "Инфо",
                            string.Format("{0}: здоровье {1}% → {2}%", shortName, before, after));

                        OperationStatus = "Восстановлено здоровье " + shortName + " (+" + heal + ")";
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        member.Health = expected;
                        CrewLog = string.Format("БД недоступна: {0}{1}{2}: изменение показано только на экране: {3} → {4}%.",
                            ShortMessage(ex), Environment.NewLine, shortName, before, member.Health);
                    }
                });
            }
        }

        public BC SetStatusCommand
        {
            get
            {
                return new BC(async () =>
                {
                    CrewMember member = SelectedCrewMember;
                    if (member == null)
                    {
                        CrewLog = "Сначала выберите члена экипажа в списке.";
                        return;
                    }

                    string status = (StatusText ?? string.Empty).Trim();
                    if (status.Length == 0)
                    {
                        CrewLog = "Введите новый статус, например «Медицинский отсек».";
                        return;
                    }

                    string before = member.Status;
                    string shortName = member.ShortName;

                    try
                    {
                        // UPDATE crew SET status = @status WHERE id = @id
                        await StationRepository.UpdateStatusAsync(member.Id, status);
                        await ReloadCrewAsync();
                        SetConnectionOk();

                        CrewLog = string.Format("{0}: статус{1}«{2}» → «{3}»{1}Операция выполнена.",
                            shortName, Environment.NewLine, before, status);

                        AddMessage("Экипаж", "Инфо",
                            string.Format("{0}: статус «{1}» → «{2}»", shortName, before, status));

                        OperationStatus = "Изменён статус " + shortName + ": " + status;
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        member.Status = status;
                        CrewLog = string.Format("БД недоступна: {0}{1}{2}: статус изменён только на экране: «{3}».",
                            ShortMessage(ex), Environment.NewLine, shortName, status);
                    }
                });
            }
        }

        public BC RefreshSystemsCommand
        {
            get
            {
                return new BC(async () =>
                {
                    try
                    {
                        // база пробуется каждый раз, а не только пока стоит флаг доступности
                        List<StationSystem> systems = await StationRepository.GetSystemsAsync();
                        Systems = new ObservableCollection<StationSystem>(systems);
                        N(nameof(Systems));
                        N(nameof(SystemsOkCount));
                        N(nameof(SystemsProblemCount));
                        SetConnectionOk();

                        OperationStatus = "Состояние систем получено из базы в " + DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        await Task.Delay(250);
                        LoadSystemsDemo();

                        OperationStatus = "Демонстрационный набор систем";
                        AddMessage("Телеметрия", "Внимание", "station_systems недоступна: " + Db.Hint(ex));
                    }
                });
            }
        }

        public BC CheckCommunicationCommand
        {
            get
            {
                return new BC(async () =>
                {
                    if (IsPinging) return;

                    IsPinging = true;
                    PingResult = "измерение...";
                    ServerTimeResult = "—";
                    SignalQuality = "проверка канала";

                    try
                    {
                        long milliseconds = 0;
                        DateTime serverTime = DateTime.Now;
                        bool fromDatabase = true;

                        try
                        {
                            // SELECT now() на сервере + замер времени ответа
                            milliseconds = await StationRepository.PingAsync();
                            serverTime = await StationRepository.GetServerTimeAsync();
                            SetConnectionOk();
                        }
                        catch (Exception ex)
                        {
                            // сервер не ответил — показываем демонстрационный замер
                            fromDatabase = false;
                            SetConnectionFailed(ex);

                            Stopwatch watch = Stopwatch.StartNew();
                            await Task.Delay(240);
                            watch.Stop();
                            milliseconds = watch.ElapsedMilliseconds;
                            serverTime = DateTime.Now;

                            AddMessage("Связь", "Внимание", "Демонстрационный замер: " + Db.Hint(ex));
                        }

                        PingResult = milliseconds + " мс";
                        ServerTimeResult = ToLocal(serverTime).ToString("dd.MM.yyyy HH:mm:ss");
                        SignalQuality = milliseconds < 400 ? "устойчивый" : "нестабильный";

                        PingHistory.Insert(0, string.Format("{0}   {1} мс   {2}",
                            DateTime.Now.ToString("HH:mm:ss"), milliseconds, SignalQuality));

                        while (PingHistory.Count > 8) PingHistory.RemoveAt(PingHistory.Count - 1);

                        OperationStatus = "Проверка связи: ответ за " + milliseconds + " мс"
                            + (fromDatabase ? " (сервер базы)" : " (демо-режим)");

                        AddMessage("Связь", milliseconds < 400 ? "Инфо" : "Внимание",
                            string.Format("Канал ЦУП — станция: задержка {0} мс", milliseconds));
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        PingResult = "нет ответа";
                        SignalQuality = "канал недоступен";
                        AddMessage("Связь", "Критично", "Сервер базы не ответил: " + ShortMessage(ex));
                    }
                    finally
                    {
                        IsPinging = false;
                    }
                });
            }
        }

        public BC LoadMessagesCommand
        {
            get
            {
                return new BC(async () =>
                {
                    if (IsLoadingMessages) return;

                    IsLoadingMessages = true;

                    try
                    {
                        // запрашиваем свежий пакет из базы, а при ошибке — демонстрационный набор
                        List<CrewMember> crew = await StationRepository.GetCrewAsync();
                        List<StationSystem> systems = await StationRepository.GetSystemsAsync();
                        int problems = systems.Count(s => s.Status != "OK");

                        AddMessage("Телеметрия", problems == 0 ? "Инфо" : "Внимание",
                            string.Format("Пакет получен из базы: экипаж {0} чел., систем {1}, замечаний {2}.",
                                crew.Count, systems.Count, problems));

                        SetConnectionOk();
                        OperationStatus = "Пакет из базы: сообщений " + Messages.Count;
                    }
                    catch (Exception ex)
                    {
                        SetConnectionFailed(ex);
                        await Task.Delay(300);
                        LoadMessagesDemo();

                        AddMessage("Связь", "Внимание", "Пакет показан демонстрационный: " + Db.Hint(ex));
                        OperationStatus = "Демонстрационный пакет сообщений";
                    }
                    finally
                    {
                        IsLoadingMessages = false;
                    }
                });
            }
        }

        public BC RunDiagnosticsCommand
        {
            get
            {
                return new BC(async () =>
                {
                    await RunDiagnosticsAsync(false);
                });
            }
        }

        public BC EmergencyCommand
        {
            get
            {
                return new BC(async () =>
                {
                    IsEmergency = true;
                    SelectSection(SectionType.Emergency);
                    AddMessage("ЦУП", "Критично", "Объявлен аварийный режим, запущена проверка всех систем");
                    await RunDiagnosticsAsync(true);
                });
            }
        }

        public BC ResetEmergencyCommand
        {
            get
            {
                return new BC(() =>
                {
                    IsEmergency = false;
                    Verdict = "Станция в штатном режиме";
                    VerdictDetail = "Аварийный режим снят оператором";
                    OperationStatus = "Аварийный режим снят в " + DateTime.Now.ToString("HH:mm:ss");
                    AddMessage("ЦУП", "Инфо", "Аварийный режим снят, станция вернулась в штатный режим");
                });
            }
        }

        public VM()
        {
            // Номера пунктов совпадают с меню консольной версии урока
            NavItems = new ObservableCollection<NavItem>
            {
                new NavItem { Section = SectionType.Crew,          Number = "1", Title = "Экипаж",             Hint = "Задания 1–3" },
                new NavItem { Section = SectionType.Systems,       Number = "2", Title = "Состояние систем",   Hint = "Задание 4" },
                new NavItem { Section = SectionType.Communication, Number = "3", Title = "Проверить связь",    Hint = "Задание 4" },
                new NavItem { Section = SectionType.Messages,      Number = "4", Title = "Сообщения станции",  Hint = "Задание 4" },
                new NavItem { Section = SectionType.Diagnostics,   Number = "5", Title = "Полная диагностика", Hint = "Задание 4" },
                new NavItem { Section = SectionType.Emergency,     Number = "6", Title = "Аварийный режим",    Hint = "Задание 5" }
            };

            Crew = new ObservableCollection<CrewMember>();
            Systems = new ObservableCollection<StationSystem>();
            Messages = new ObservableCollection<StationMessage>();
            DiagnosticResults = new ObservableCollection<DiagnosticLine>();
            PingHistory = new ObservableCollection<string>();

            damageText = "30";
            healText = "20";
            statusText = "Медицинский отсек";
            crewLog = "Выберите члена экипажа — операции станут доступны.";

            pingResult = "нет данных";
            serverTimeResult = "—";
            signalQuality = "нет данных";

            verdict = "Диагностика не запускалась";
            verdictDetail = "Нажмите «Запустить диагностику»";

            connectionInfo = Db.Provider.DisplayName + " · подключение...";

            selectedNavItem = NavItems[0];

            // Пока идёт подключение к БД, в интерфейсе уже есть данные
            LoadCrewDemo();
            LoadSystemsDemo();
            LoadMessagesDemo();

            MissionTime = "T+ 00:00:00";
            OperationStatus = "Система готова. Ожидание команды оператора.";

            clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            clock.Tick += Clock_Tick;
            clock.Start();
        }

        void Clock_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startedAt;
            MissionTime = string.Format("T+ {0:00}:{1:00}:{2:00}", elapsed.TotalHours, elapsed.Minutes, elapsed.Seconds);
        }

        public async Task InitializeAsync()
        {
            OperationStatus = "Подключение к базе данных...";

            try
            {
                List<CrewMember> crew = await StationRepository.GetCrewAsync();
                List<StationSystem> systems = await StationRepository.GetSystemsAsync();

                ApplyCrew(crew, true);

                Systems = new ObservableCollection<StationSystem>(systems);
                N(nameof(Systems));
                N(nameof(SystemsOkCount));
                N(nameof(SystemsProblemCount));

                IsDbAvailable = true;
                SetConnectionOk();

                CrewLog = string.Format("Данные получены из базы: {0} членов экипажа, {1} систем.", crew.Count, systems.Count);
                OperationStatus = "Данные загружены из PostgreSQL в " + DateTime.Now.ToString("HH:mm:ss");

                AddMessage("ЦУП", "Инфо",
                    string.Format("Сеанс связи с базой установлен: экипаж {0} чел., систем {1}.", crew.Count, systems.Count));
            }
            catch (Exception ex)
            {
                SetConnectionFailed(ex);
                CrewLog = string.Format("База данных недоступна: {0}{1}{2}{1}Показан демонстрационный набор данных.",
                    ShortMessage(ex), Environment.NewLine, Db.Hint(ex));
                OperationStatus = "Работа в демонстрационном режиме";

                AddMessage("База данных", "Критично", "Нет подключения: " + ShortMessage(ex));
                AddMessage("База данных", "Внимание", Db.Hint(ex));
            }
        }

        void SelectSection(SectionType section)
        {
            NavItem item = NavItems.FirstOrDefault(n => n.Section == section);
            if (item != null) SelectedNavItem = item;
        }

        static int ParseAmount(string text)
        {
            int value;
            if (int.TryParse((text ?? string.Empty).Trim(), out value)) return value;
            return -1;
        }

        // Значение из списка, который только что перечитан из базы
        int ReadHealth(int id, int fallback)
        {
            CrewMember member = allCrew == null ? null : allCrew.FirstOrDefault(c => c.Id == id);
            return member == null ? fallback : member.Health;
        }

        void SetConnectionOk()
        {
            IsDbAvailable = true;
            ConnectionInfo = string.Format("{0} · подключено · экипаж: {1}, системы: {2}",
                Db.Provider.DisplayName,
                allCrew == null ? 0 : allCrew.Count,
                Systems == null ? 0 : Systems.Count);
        }

        void SetConnectionFailed(Exception ex)
        {
            IsDbAvailable = false;
            ConnectionInfo = Db.Provider.DisplayName + " · нет доступа" + Db.ErrorCode(ex);
        }

        // now() возвращает время в UTC — показываем местное
        static DateTime ToLocal(DateTime serverTime)
        {
            return serverTime.Kind == DateTimeKind.Utc ? serverTime.ToLocalTime() : serverTime;
        }

        static string ShortMessage(Exception ex)
        {
            string message = (ex.Message ?? string.Empty).Replace(Environment.NewLine, " ");
            return message.Length > 110 ? message.Substring(0, 110) + "..." : message;
        }

        // Бортовой журнал: демо-набор плюс реальные события
        void AddMessage(string source, string level, string text)
        {
            Messages.Insert(0, new StationMessage
            {
                Time = DateTime.Now.ToString("HH:mm"),
                Source = source,
                Level = level,
                Text = text
            });

            while (Messages.Count > 40) Messages.RemoveAt(Messages.Count - 1);
        }

        async Task ReloadCrewAsync()
        {
            string query = (crewSearch ?? string.Empty).Trim();

            List<CrewMember> list = string.IsNullOrEmpty(query)
                ? await StationRepository.GetCrewAsync()
                : await StationRepository.FindCrewAsync(query);

            ApplyCrew(list, string.IsNullOrEmpty(query));
        }

        void ApplyCrew(List<CrewMember> list, bool isFullList)
        {
            if (isFullList) allCrew = list;

            int keepId = selectedCrewMember == null ? 0 : selectedCrewMember.Id;

            Crew = new ObservableCollection<CrewMember>(list);
            N(nameof(Crew));
            N(nameof(CrewOnBoard));

            CrewMember keep = keepId == 0 ? null : Crew.FirstOrDefault(c => c.Id == keepId);
            SelectedCrewMember = keep ?? Crew.FirstOrDefault();
        }

        void StartCrewSearch()
        {
            if (searchCts != null) searchCts.Cancel();

            CancellationTokenSource cts = new CancellationTokenSource();
            searchCts = cts;
            CrewSearchTask(cts);
        }

        async void CrewSearchTask(CancellationTokenSource cts)
        {
            try
            {
                // небольшая задержка, чтобы не отправлять запрос на каждую букву
                await Task.Delay(250, cts.Token);
                await SearchCrewAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // пользователь продолжает печатать — предыдущий запрос отменён
            }
            catch (Exception ex)
            {
                SetConnectionFailed(ex);
                CrewLog = "Ошибка поиска: " + ShortMessage(ex);
            }
        }

        async Task SearchCrewAsync(CancellationToken token)
        {
            string query = (crewSearch ?? string.Empty).Trim();

            if (!IsDbAvailable)
            {
                // демонстрационный режим: фильтруем тот же набор в памяти
                FilterCrewLocal(query);
                return;
            }

            // WHERE name ILIKE @name (пустой запрос — весь список)
            List<CrewMember> found = string.IsNullOrEmpty(query)
                ? await StationRepository.GetCrewAsync()
                : await StationRepository.FindCrewAsync(query);

            if (token.IsCancellationRequested) return;

            ApplyCrew(found, string.IsNullOrEmpty(query));
            SetConnectionOk();

            CrewLog = string.IsNullOrEmpty(query)
                ? string.Format("Показан весь экипаж: {0} чел.", found.Count)
                : string.Format("Поиск «{0}»: найдено {1} чел.", query, found.Count);
        }

        void FilterCrewLocal(string query)
        {
            if (allCrew == null) return;

            IEnumerable<CrewMember> filtered = string.IsNullOrEmpty(query)
                ? allCrew
                : allCrew.Where(c =>
                    c.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.Role.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.Status.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);

            ApplyCrew(filtered.ToList(), string.IsNullOrEmpty(query));
        }

        void LoadCrewDemo()
        {
            List<CrewMember> demo = new List<CrewMember>
            {
                new CrewMember { Id = 1, Name = "Александр Малыгин",   Role = "Командир",        Health = 100, Status = "На станции" },
                new CrewMember { Id = 2, Name = "Талина Захарова",     Role = "Бортинженер",     Health = 100, Status = "На станции" },
                new CrewMember { Id = 3, Name = "Владислав Чисталев",  Role = "Пилот",           Health = 92,  Status = "На станции" },
                new CrewMember { Id = 4, Name = "Никита Андрушкевич",  Role = "Инженер",         Health = 84,  Status = "На станции" },
                new CrewMember { Id = 5, Name = "Данил Пономарев",     Role = "Медик",           Health = 97,  Status = "На станции" },
                new CrewMember { Id = 6, Name = "Иосиф Колегов",       Role = "Инженер",         Health = 71,  Status = "На станции" },
                new CrewMember { Id = 7, Name = "Алексей Григорун",    Role = "Оператор связи",  Health = 63,  Status = "Медицинский отсек" }
            };

            ApplyCrew(demo, true);
        }

        void LoadSystemsDemo()
        {
            Systems = new ObservableCollection<StationSystem>
            {
                new StationSystem { Id = 1, Name = "Двигатели",        Status = "OK",      Detail = "Тяга в норме, расход топлива 1.2 кг/сут", LastCheck = "демо-набор" },
                new StationSystem { Id = 2, Name = "Жизнеобеспечение", Status = "OK",      Detail = "O₂ 21%, CO₂ 0.04%, влажность 45%",        LastCheck = "демо-набор" },
                new StationSystem { Id = 3, Name = "Навигация",        Status = "WARNING", Detail = "Отклонение курса 0.4°, гироскоп №3 прогрет", LastCheck = "демо-набор" },
                new StationSystem { Id = 4, Name = "Связь",            Status = "OK",      Detail = "Канал 12 Мбит/с, задержка 240 мс",        LastCheck = "демо-набор" },
                new StationSystem { Id = 5, Name = "Энергосистема",    Status = "OK",      Detail = "Солнечные панели 84%, АКБ 96%",           LastCheck = "демо-набор" },
                new StationSystem { Id = 6, Name = "Терморегуляция",   Status = "WARNING", Detail = "Контур B: 27.4 °C, норма до 26 °C",       LastCheck = "демо-набор" },
                new StationSystem { Id = 7, Name = "Стыковочный узел", Status = "OK",      Detail = "Герметичность подтверждена",              LastCheck = "демо-набор" }
            };

            N(nameof(Systems));
            N(nameof(SystemsOkCount));
            N(nameof(SystemsProblemCount));
        }

        void LoadMessagesDemo()
        {
            Messages = new ObservableCollection<StationMessage>
            {
                new StationMessage { Time = "09:41", Source = "Связь",      Level = "Инфо",     Text = "Телеметрический поток стабилен, потерь пакетов нет." },
                new StationMessage { Time = "09:27", Source = "Навигация",  Level = "Внимание", Text = "Отклонение курса 0.4° — выполняется корректировка." },
                new StationMessage { Time = "09:05", Source = "СЖО",        Level = "Инфо",     Text = "Регенерация кислорода завершена, уровень O₂ 21%." },
                new StationMessage { Time = "08:52", Source = "Экипаж",     Level = "Инфо",     Text = "Плановый медицинский осмотр завершён, замечаний нет." },
                new StationMessage { Time = "08:30", Source = "Энергетика", Level = "Критично", Text = "Контур B: рост температуры, включено резервное охлаждение." }
            };

            N(nameof(Messages));
        }

        async Task RunDiagnosticsAsync(bool emergency)
        {
            if (IsDiagnosticsRunning) return;

            IsDiagnosticsRunning = true;
            DiagnosticResults.Clear();
            CrewProgress = 0;
            SystemsProgress = 0;
            CommunicationProgress = 0;
            TotalProgress = 0;

            Verdict = emergency ? "АВАРИЙНАЯ ДИАГНОСТИКА ВЫПОЛНЯЕТСЯ" : "Диагностика выполняется...";
            VerdictDetail = "Опрос экипажа, подсистем и канала связи";
            OperationStatus = "Запущена диагностика в " + DateTime.Now.ToString("HH:mm:ss");

            try
            {
                // Progress<T> создан в UI-потоке, поэтому значения сразу попадают в интерфейс
                Progress<int> crewReporter = new Progress<int>(value => { CrewProgress = value; UpdateTotal(); });
                Progress<int> systemsReporter = new Progress<int>(value => { SystemsProgress = value; UpdateTotal(); });
                Progress<int> communicationReporter = new Progress<int>(value => { CommunicationProgress = value; UpdateTotal(); });

                // Три операции запускаются одновременно — главная идея урока
                Task<DiagnosticLine> crewTask = CheckCrewAsync(crewReporter);
                Task<DiagnosticLine> systemsTask = CheckSystemsAsync(systemsReporter);
                Task<DiagnosticLine> communicationTask = CheckCommunicationAsync(communicationReporter);

                DiagnosticLine[] results = await Task.WhenAll(crewTask, systemsTask, communicationTask);

                foreach (DiagnosticLine line in results)
                {
                    DiagnosticResults.Add(line);
                }

                DiagnosticResults.Add(BuildNavigationLine());

                int problems = DiagnosticResults.Count(l => l.Status != "OK");

                if (problems == 0)
                {
                    Verdict = emergency ? "АВАРИЯ ЛОЖНАЯ: СТАНЦИЯ РАБОТОСПОСОБНА" : "Станция работоспособна";
                }
                else
                {
                    Verdict = emergency
                        ? "АВАРИЙНЫЙ РЕЖИМ: обнаружено отклонений — " + problems
                        : "ВНИМАНИЕ: отклонений — " + problems;
                }

                VerdictDetail = string.Format("Проверок: {0}, из них с замечаниями: {1}. Завершено в {2}",
                    DiagnosticResults.Count, problems, DateTime.Now.ToString("HH:mm:ss"));

                OperationStatus = "Диагностика завершена в " + DateTime.Now.ToString("HH:mm:ss");

                AddMessage("Диагностика", problems == 0 ? "Инфо" : "Внимание",
                    string.Format("Проверено узлов: {0}, замечаний: {1}. Режим: {2}",
                        DiagnosticResults.Count, problems, IsDbAvailable ? "база данных" : "демонстрационный"));
            }
            catch (Exception ex)
            {
                SetConnectionFailed(ex);
                Verdict = "ДИАГНОСТИКА ПРЕРВАНА";
                VerdictDetail = "Ошибка: " + ShortMessage(ex);
                AddMessage("Диагностика", "Критично", "Диагностика прервана: " + ShortMessage(ex));
            }
            finally
            {
                IsDiagnosticsRunning = false;
            }
        }

        void UpdateTotal()
        {
            TotalProgress = (CrewProgress + SystemsProgress + CommunicationProgress) / 3;
        }

        // Проверка экипажа: читаем таблицу crew
        async Task<DiagnosticLine> CheckCrewAsync(IProgress<int> progress)
        {
            if (!IsDbAvailable) return await CheckCrewDemoAsync(progress);

            try
            {
                progress.Report(25);

                List<CrewMember> crew = await StationRepository.GetCrewAsync();

                progress.Report(100);

                if (crew.Count == 0)
                {
                    return new DiagnosticLine { Name = "Экипаж", Status = "WARNING", Detail = "таблица crew пуста" };
                }

                int minimum = crew.Min(c => c.Health);
                int injured = crew.Count(c => c.Health < 60);

                return new DiagnosticLine
                {
                    Name = "Экипаж",
                    Status = injured == 0 ? "OK" : "WARNING",
                    Detail = string.Format("{0} чел. в таблице crew, минимальное здоровье {1}%", crew.Count, minimum)
                };
            }
            catch (Exception ex)
            {
                progress.Report(100);
                SetConnectionFailed(ex);

                return new DiagnosticLine
                {
                    Name = "Экипаж",
                    Status = "CRITICAL",
                    Detail = "ошибка запроса: " + ShortMessage(ex)
                };
            }
        }

        // Проверка систем: читаем таблицу station_systems
        async Task<DiagnosticLine> CheckSystemsAsync(IProgress<int> progress)
        {
            if (!IsDbAvailable) return await CheckSystemsDemoAsync(progress);

            try
            {
                progress.Report(25);

                List<StationSystem> systems = await StationRepository.GetSystemsAsync();

                progress.Report(100);

                Systems = new ObservableCollection<StationSystem>(systems);
                N(nameof(Systems));
                N(nameof(SystemsOkCount));
                N(nameof(SystemsProblemCount));

                int warnings = systems.Count(s => s.Status != "OK");

                return new DiagnosticLine
                {
                    Name = "Двигатели",
                    Status = warnings == 0 ? "OK" : "WARNING",
                    Detail = string.Format("проверено систем: {0}, замечаний: {1}", systems.Count, warnings)
                };
            }
            catch (Exception ex)
            {
                progress.Report(100);
                SetConnectionFailed(ex);

                return new DiagnosticLine
                {
                    Name = "Двигатели",
                    Status = "CRITICAL",
                    Detail = "ошибка запроса: " + ShortMessage(ex)
                };
            }
        }

        // Проверка связи: SELECT now() и замер времени ответа
        async Task<DiagnosticLine> CheckCommunicationAsync(IProgress<int> progress)
        {
            if (!IsDbAvailable) return await CheckCommunicationDemoAsync(progress);

            try
            {
                progress.Report(40);

                long milliseconds = await StationRepository.PingAsync();
                DateTime serverTime = await StationRepository.GetServerTimeAsync();

                progress.Report(100);

                PingResult = milliseconds + " мс";
                ServerTimeResult = serverTime.ToString("dd.MM.yyyy HH:mm:ss");
                SignalQuality = milliseconds < 400 ? "устойчивый" : "нестабильный";

                return new DiagnosticLine
                {
                    Name = "Связь",
                    Status = milliseconds < 800 ? "OK" : "WARNING",
                    Detail = string.Format("SELECT now() выполнен за {0} мс, время сервера {1}",
                        milliseconds, ToLocal(serverTime).ToString("HH:mm:ss"))
                };
            }
            catch (Exception ex)
            {
                progress.Report(100);
                SetConnectionFailed(ex);

                return new DiagnosticLine
                {
                    Name = "Связь",
                    Status = "CRITICAL",
                    Detail = "ошибка запроса: " + ShortMessage(ex)
                };
            }
        }

        DiagnosticLine BuildNavigationLine()
        {
            StationSystem navigation = Systems.FirstOrDefault(s => s.Name == "Навигация");
            bool warning = navigation != null && navigation.Status != "OK";

            return new DiagnosticLine
            {
                Name = "Навигация",
                Status = warning ? "WARNING" : "OK",
                Detail = navigation == null ? "нет данных в station_systems" : navigation.Detail
            };
        }

        async Task<DiagnosticLine> CheckCrewDemoAsync(IProgress<int> progress)
        {
            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(70);
                progress.Report(i);
            }

            int minimum = allCrew.Min(c => c.Health);
            int injured = allCrew.Count(c => c.Health < 60);

            return new DiagnosticLine
            {
                Name = "Экипаж",
                Status = injured == 0 ? "OK" : "WARNING",
                Detail = string.Format("{0} чел. (демо-набор), минимальное здоровье {1}%", allCrew.Count, minimum)
            };
        }

        async Task<DiagnosticLine> CheckSystemsDemoAsync(IProgress<int> progress)
        {
            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(90);
                progress.Report(i);
            }

            int warnings = Systems.Count(s => s.Status != "OK");

            return new DiagnosticLine
            {
                Name = "Двигатели",
                Status = warnings == 0 ? "OK" : "WARNING",
                Detail = string.Format("проверено систем: {0}, замечаний: {1} (демо-набор)", Systems.Count, warnings)
            };
        }

        async Task<DiagnosticLine> CheckCommunicationDemoAsync(IProgress<int> progress)
        {
            for (int i = 0; i <= 100; i += 20)
            {
                await Task.Delay(60);
                progress.Report(i);
            }

            return new DiagnosticLine
            {
                Name = "Связь",
                Status = "OK",
                Detail = "канал устойчив (демонстрационная проверка)"
            };
        }
    }
}
