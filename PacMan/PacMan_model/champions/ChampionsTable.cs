﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PacMan_model.util;

namespace PacMan_model.champions {

    public class ChampionsTable : IChampionsTable {

        private static readonly string RootDir = Directory.GetCurrentDirectory();
        private static readonly string PathToChampions = RootDir + "\\" /*+ "\\Champions"*/;
        private static readonly string ChampionsFileName = PathToChampions + "Champions.txt";
        private const string Separator = " ";

        private const int MaxRecords = 10;
        private readonly IList<Tuple<int, string>> _championsRecords = new List<Tuple<int, string>>(MaxRecords);

        private bool _isInit;

        public ChampionsTable() {
            LoadFromFile();
        }

        public void LoadFromFile() {

            if (!File.Exists(ChampionsFileName)) {
                return;
            }
            using (var input = File.OpenText(ChampionsFileName)) {

                while (!input.EndOfStream) {

                    var readLine = input.ReadLine();

                    if (null == readLine) {
                        throw new InvalidChampionsSource("unexpected end of file: " + PathToChampions);
                    }

                    var newLine = readLine.Split(Separator.ToCharArray());

                    try {
                        var name = newLine[0];
                        var score = int.Parse(newLine[1]);

                        AddNewResult(name, score);
                    }
                    catch (ArgumentException) {
                        throw new InvalidChampionsSource("invalid line: " + PathToChampions);
                    }
                    catch (FormatException) {
                        throw new InvalidChampionsSource("invalid line: " + PathToChampions);
                    }
                }
            }

            _isInit = true;

        }

        public void SaveToFile() {


            using (var output = new StreamWriter(File.Create(ChampionsFileName))) {

                foreach (var championsRecord in _championsRecords) {
                    output.WriteLine(championsRecord.Item2 + Separator + championsRecord.Item1);
                }

                output.Flush();
            }

        }

        public bool IsNewRecord(int result) {

            if (result <= 0) {
                return false;
            }

            return _championsRecords.Count < MaxRecords || _championsRecords.Any(record => record.Item1 < result);
        }

        public void AddNewResult(string name, int result) {
            if (result <= 0) {
                throw new ArgumentNullException("result");
            }
            if (null == name) {
                throw new ArgumentNullException("name");
            }
            AddNewResult(result, name);
        }

        public void AddNewResult(int result, string name) {
            if (result <= 0) {
                throw new ArgumentNullException("result");
            }
            if (null == name) {
                throw new ArgumentNullException("name");
            }


            var newRecord = new Tuple<int, string>(result, name);

            if (false == IsNewRecord(result)) {
                throw new ArgumentException("score is not enough");
            }

            var newIndex = -1;

            for (var index = 0; index < _championsRecords.Count; index++) {

                if (result > _championsRecords[index].Item1) {
                    newIndex = index;
                    break;
                }
            }

            if (_championsRecords.Count == MaxRecords) {
                _championsRecords.Remove(_championsRecords.Last());
            }

            if (-1 == newIndex) {
                //  add in the end
                _championsRecords.Add(newRecord);
            }
            else {
                //  insert in specific place
                _championsRecords.Insert(newIndex, newRecord);
            }

            NotifyChangedStatement();
        }

        public event EventHandler<ChampionsTableChangedEventArs> ChampionsTableState;

        public void ForceNotify() {
            NotifyChangedStatement();
        }

        private void NotifyChangedStatement() {


            if (!_isInit) {
                return;
            }

            OnStatementChangedNotify(new ChampionsTableChangedEventArs(_championsRecords));
        }

        protected virtual void OnStatementChangedNotify(ChampionsTableChangedEventArs e) {

            if (null == e) {
                throw new ArgumentNullException("e");
            }

            e.Raise(this, ref ChampionsTableState);
        }

        public ICollection<Tuple<int, string>> GetResults() {
            return _championsRecords;
        }


        public void Dispose() {
            SaveToFile();
        }
    }
}