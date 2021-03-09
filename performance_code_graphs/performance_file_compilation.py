import os
import csv

# --- Sources: 
# https://www.geeksforgeeks.org/working-csv-files-python/

# UPDATE THIS when running the code below
PROJECT_PATH = ""
RAW_DATA_FILES_FOLDER = ""

class PerformanceTracker:

    path_proj = PROJECT_PATH
    path_perf = path_proj + "/" + RAW_DATA_FILES_FOLDER
    path_output = os.path.dirname(os.path.realpath(__file__))

    all_files = []
    summary_files = []
    raw_files = []

    summaryCSVHeaders = []
    rawCSVHeaders = []

    summaryCSVDataAgg = []
    rawCSVDataAgg = []
    
    def __init__(self):
        print("Hi")

    def fillFileNames(self):
        self.all_files = os.listdir(self.path_perf)
        for file in self.all_files:
            if(file[0:3] == "sum"):
                self.summary_files.append(file)
            elif(file[0:3] == "raw"):
                self.raw_files.append(file)

    def aggregateCSVData(self):
        for file in self.summary_files:
            with open(self.path_perf + "/" + file, "r") as file:
                csvreader = csv.reader(file)
                headers = next(csvreader)
                if(len(self.summaryCSVHeaders) == 0):
                    self.summaryCSVHeaders = headers
                for row in csvreader:
                    self.summaryCSVDataAgg.append(row)
                    print(row)

        for file in self.raw_files:
            with open(self.path_perf + "/" + file, "r") as file:
                csvreader = csv.reader(file)
                headers = next(csvreader)
                if(len(self.rawCSVHeaders) == 0):
                    self.rawCSVHeaders = headers
                for row in csvreader:
                    self.rawCSVDataAgg.append(row)

    def writeCSVDataToAggFiles(self, fileName, dataList, headerList):
        with open(fileName, "w", newline='') as file:
            csvwriter = csv.writer(file)
            csvwriter.writerow(headerList)
            csvwriter.writerows(dataList)

    def writeCSVData(self):
        self.writeCSVDataToAggFiles(self.path_output + "/" + "summary_agg.csv",
                               self.summaryCSVDataAgg, 
                               self.summaryCSVHeaders)

        self.writeCSVDataToAggFiles(self.path_output + "/" + "raw_agg.csv",
                               self.rawCSVDataAgg, 
                               self.rawCSVHeaders)


test = PerformanceTracker()
test.fillFileNames()
test.aggregateCSVData()
test.writeCSVData()
