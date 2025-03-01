﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.ViewModel {
    partial class MainViewModel {

        public void Loaded() {
        }

        public void SortFiles() {
            SuppressFileTreeRefresh = true;
            RootItem?.Sort(FileComparer.Compare);
            SuppressFileTreeRefresh = false;
        }

        public void SortExtensions() {
            Extensions.Sort(ExtensionComparer.Compare);
        }

        public void ActivateItem() {
            // TODO: Determine the default action for each item type
        }

        public void UpdateEmptyRecycleBin() {
            UpdateEmptyRecycleBin(false);
        }

        public void UpdateEmptyRecycleBin(bool force) {
            if (!ReadyForUpdate())
                return;

            recycleInfoTask = Task.Run(UpdateEmptyRecycleBinThread);
            return;

            bool NotAlreadyRunning() => recycleInfoTask?.IsCompleted ?? true;
            bool TimerIsReady() => lastRecycleWatch == null || lastRecycleWatch.Elapsed > TimeSpan.FromSeconds(1);
            bool ReadyForUpdate() => force || (NotAlreadyRunning() && TimerIsReady());
        }
        private void UpdateEmptyRecycleBinThread() {
            Debug.WriteLine("UpdateEmptyRecycleBin");
            RecycleBinInfo info = OS.GetAllRecycleBinInfo();
            string label = "Empty Recycle Bins";
            if (info != null) {
                label += " (";
                if (info.ItemCount == 0 && info.Size == 0) {
                    label += "Empty";
                }
                else {
                    label += $"{info.ItemCount:N0} ";
                    if (info.ItemCount == 1)
                        label += $"Item";
                    else
                        label += $"Items";
                    label += $", {FormatBytes.Format(info.Size)}";
                }
                label += ")";
            }
            EmptyRecycleBinLabel = label;
            allRecycleBinInfo = info;
            Application.Current.Dispatcher.Invoke(EmptyRecycleBin.NotifyCanExecuteChanged);
            lastRecycleWatch ??= new Stopwatch();
            lastRecycleWatch.Restart();
        }

        public void WindowShown() {
            Open.Execute();
        }
    }
}
