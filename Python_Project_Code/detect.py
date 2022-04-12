#region import 依賴

import numpy
import torch
import torch.backends.cudnn as cudnn
import sys
sys.path.insert(0, './yolov5')
from yolov5.models.experimental import attempt_load
from yolov5.utils.datasets import LoadScreen_Capture
from yolov5.utils.general import check_img_size, non_max_suppression, scale_coords, \
    check_imshow
from yolov5.utils.torch_utils import select_device, time_synchronized
from deep_sort_pytorch.utils.parser import get_config
from deep_sort_pytorch.deep_sort import DeepSort
import argparse
import os,io
import shutil
import urllib
import time
from pathlib import Path
import cv2

from mss import mss
import os
import threading
from tkinter_test import *
#from ui_main import *
from win32 import win32api, win32gui
from win32.lib import win32con
import win32gui, win32ui, win32api, win32con
from win32.win32api import GetSystemMetrics
from datetime import datetime
import os,shutil,sys
from datetime import timezone
from PyQt5 import QtCore, QtGui, QtWidgets
from PyQt5 import QtWidgets
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtGui import *
# selenium 爬蟲插件 用於抓取cctv串流圖片
from selenium import webdriver
from webdriver_manager.firefox import GeckoDriverManager
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.firefox.options import Options
from selenium.common.exceptions import TimeoutException
from selenium.common.exceptions import WebDriverException
from types import SimpleNamespace as Namespace
#from playsound import playsound
from pygame import mixer
import webbrowser
import atexit

import json
#endregion


def detect_function(self,opt,mode,url):
    out, source, weights, show_vid, save_vid, save_txt, imgsz = \
        opt.output, opt.source, opt.weights, opt.show_vid, opt.save_vid, opt.save_txt, opt.img_size
    webcam = source == '0' or source.startswith(
        'rtsp') or source.startswith('http') or source.endswith('.txt')
    self.sv.total_detect_count = 0
    cfg = get_config()
    cfg.merge_from_file(opt.config_deepsort)
    deepsort = DeepSort(cfg.DEEPSORT.REID_CKPT,
                        max_dist=cfg.DEEPSORT.MAX_DIST, min_confidence=cfg.DEEPSORT.MIN_CONFIDENCE,
                        nms_max_overlap=cfg.DEEPSORT.NMS_MAX_OVERLAP, max_iou_distance=cfg.DEEPSORT.MAX_IOU_DISTANCE,
                        max_age=cfg.DEEPSORT.MAX_AGE, n_init=cfg.DEEPSORT.N_INIT, nn_budget=cfg.DEEPSORT.NN_BUDGET,
                        use_cuda=True)
            
    device = select_device(opt.device)
    if os.path.exists(out):
        shutil.rmtree(out)  # delete output folder
    os.makedirs(out)  # make new output folder
    half = device.type != 'cpu'  # half precision only supported on CUDA
    #imgsz *= self.img_size_zoom
    # Load model
    model = attempt_load(weights, map_location=device)  # load FP32 model
    stride = int(model.stride.max())  # model stride
    imgsz = check_img_size(imgsz, s=stride)  # check img_size
    names = model.module.names if hasattr(model, 'module') else model.names  # get class names
    if half:
        model.half()  # to FP16

    if show_vid:
        show_vid = check_imshow()
    names = model.module.names if hasattr(model, 'module') else model.names

    # Run inference
    if device.type != 'cpu':
        model(torch.zeros(1, 3, imgsz, imgsz).to(device).type_as(next(model.parameters())))  # run once
    
    #region 爬蟲用變數
    location = None
    size = None
    src = None
    #endregion
    
    #start_time = time_synchronized()
    #region 區域截屏用變數
    sx=None
    sy=None
    ex=None
    ey=None
    monitor_number = opt.monitor_num
    sct = None
    monitor_info = None

    
    
    #endregion
    if  ((mode == "yt") or (mode == "cctv")):
        #region 初始化 selenium webdriver and url 判別 (水利署與公路總局)
        options = Options()
        if(self.sv.g_isheadless is True):
            options.add_argument('-headless')
        #採用無頭模式的FIREFOX 不採用chrome 因為無頭模式下有問題
        
        self.g_webdriver = webdriver.Firefox(executable_path=GeckoDriverManager().install(),options=options)
        #now_log_out = logging.getLogger("WDM")
        self.g_webdriver.get(url)
        try:
            WebDriverWait(self.g_webdriver, 15).until(EC.element_to_be_clickable(
            (By.XPATH, "//button[@class='ytp-large-play-button ytp-button']"))).click()
            # WebDriverWait(driver,5).until(EC.element_to_be_clickable((By.CLASS_NAME,'ytp-iv-video-content')))
        except TimeoutException as ex:
            print('out of time')
            sys.exit(1)
        # time.sleep(10)

        if(mode == "yt"):
            youtube_panel_xy = None
            try:
                youtube_panel_xy = self.g_webdriver.find_element_by_xpath("//div[@class='ytp-iv-video-content']")
            except:
                try:
                    print("'ytp-iv-video-content' not found try find player-api")
                    youtube_panel_xy = self.g_webdriver.find_element_by_xpath("//div[@id='player-container-inner']")
                except: 
                    self.now_log_out = " 找不到YOUTUBE 撥放器HTML，請嘗試重新讀取"
            location = youtube_panel_xy.location
            size  = youtube_panel_xy.size
        elif(mode == 'cctv'):
            if 'cctv.aspx' in url :        
                self.g_webdriver.get(url)
                #time.sleep(10)

                try:
                    WebDriverWait(self.g_webdriver,10).until(EC.element_to_be_clickable((By.ID,'frmMain')))
                except TimeoutException as ex:
                    print(ex.message)
                iframe_target = self.g_webdriver.find_element_by_xpath('//iframe[@id="frmMain"]')
                src = iframe_target.get_attribute('src')
                print(src)
                url = src
                self.g_webdriver.get(src)
            
            try:
                WebDriverWait(self.g_webdriver,10).until(EC.element_to_be_clickable((By.ID,'cctv_image')))
            except TimeoutException as ex:
                print(ex.message)

            img = self.g_webdriver.find_element_by_xpath('//div[@id="cctv_image"]/img')
            location = img.location
            size  = img.size
            # print('l:',location)
            # print('s:',size)
            #png = driver.get_screenshot_as_png()
            src = img.get_attribute('src')
        
        size  = youtube_panel_xy.size
        print('l:',location)
        print('s:',size)
            #png = driver.get_screenshot_as_png()
        #src = img.get_attribute('src')
    frame_idx = 0
    start_time = time_synchronized()
    #endregion
    #for frame_idx, (path, img, im0s, vid_cap) in enumerate(dataset):
    if (mode == "area"):
        sx=int(self.srs.box_area[0])
        sy=int(self.srs.box_area[1])
        ex=int(self.srs.box_area[2])
        ey=int(self.srs.box_area[3])
        sct = mss()
        monitor_info = {'top' : sy, 'left' : sx, 'width' : ex-sx, 'height' : ey-sy,
        "mon": monitor_number
        }
        mon = sct.monitors[monitor_number]

    self.sv.total_detect_count = 0
    self.total_unique_id_dict.clear()

    last_img_size = self.sv.img_size_zoom
    sct_img = None
    while (self.src_state == 1):  
        dt = datetime.now()
        
        time.sleep(self.sv.detect_interval_time)
        # if(detecting==True):
        #     break
        frame_idx = frame_idx + 1

        
        if (mode == "yt"):
            try:
                png = self.g_webdriver.get_screenshot_as_png()
            except WebDriverException:
                self.now_log_out = "使用者關閉瀏覽器..."
                break
            nparr = numpy.frombuffer(png, numpy.uint8)
            img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
            left = location['x']
            top = location['y']
            right = location['x'] + size['width']
            bottom = location['y'] + size['height']
            sct_img = img[top:int(bottom), left:int(right)]
        elif (mode=='cctv'):
            if 'jpg'in src or 'jpeg'in src or 'png'in src or 'JPEG'in src or 'JPG' in src:
                # 水利署影像為圖片檔，直接進行讀取
                try:
                    img = self.g_webdriver.find_element_by_xpath('//div[@id="cctv_image"]/img')
                except WebDriverException:
                    self.now_log_out = "使用者關閉瀏覽器..."
                    break
                src = img.get_attribute('src')
                try:
                    req = urllib.request.urlopen(src)
                except :
                    self.now_log_out = '請求網頁連結失敗，重新讀取'
                    continue
                arr = numpy.asarray(bytearray(req.read()), dtype=numpy.uint8)
                sct_img = cv2.imdecode(arr, -1)
            else:
                # 公路總局影像非圖片檔，直接使用擷取圖片
                png = self.g_webdriver.get_screenshot_as_png()
                nparr = numpy.frombuffer(png, numpy.uint8)
                img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
                left = location['x']
                top = location['y']
                right = location['x'] + size['width']
                bottom = location['y'] + size['height']
                sct_img = img[top:int(bottom), left:int(right)]
        elif (mode == 'area'):
            sct_img = sct.grab(monitor_info)
            sct_img = numpy.array(sct_img)
            sct_img = cv2.cvtColor(sct_img, cv2.COLOR_RGBA2RGB)
        
        
        #放大倍率功能 同時放大模型所需的內建imgsz 
        if(self.sv.img_size_zoom != 1):
            #print("放大倍率")
            sct_img = cv2.resize(sct_img,(0,0),fx=float(self.sv.img_size_zoom),fy=float(self.sv.img_size_zoom),interpolation=cv2.INTER_CUBIC)
            if(last_img_size!=self.sv.img_size_zoom ):
                imgsz = imgsz * (1/last_img_size)
                print(imgsz,last_img_size,self.sv.img_size_zoom)
                imgsz = int(imgsz *self.sv.img_size_zoom)
                last_img_size = self.sv.img_size_zoom
        dataset = LoadScreen_Capture(sct_img,img_size=imgsz)
        time.sleep(0.01)
        img, im0  = dataset.__next__()
        img = torch.from_numpy(img).to(device)
        img = img.half() if half else img.float()  # uint8 to fp16/32
        img /= 255.0  # 0 - 255 to 0.0 - 1.0
        if img.ndimension() == 3:
            img = img.unsqueeze(0)
        #print(img.shape)
        #print(img.mode)
        # Inference
        t1 = time_synchronized()
        
        pred = model(img, augment=opt.augment)[0]

        # Apply NMS
        pred = non_max_suppression(
            pred, opt.conf_thres, opt.iou_thres, classes=opt.classes, agnostic=opt.agnostic_nms)
        t2 = time_synchronized()
        self._time_counter += (t2-t1)
        # Process detections
        for i, det in enumerate(pred):  # detections per image
            #p, s, im0 = path, '', im0s
            s = ''

            if det is not None and len(det):
                # Rescale boxes from img_size to im0 size
                det[:, :4] = scale_coords(
                    img.shape[2:], det[:, :4], im0.shape).round()
                #print("unique : det[:, -1].unique()")
                # Print results
                for c in det[:, -1].unique():
                    
                    n = (det[:, -1] == c).sum()  # detections per class
                    s += '%g %ss, ' % (n, names[int(c)])  # add to string
                    #print("in ? ",s)

                xywh_bboxs = []
                confs = []

                # Adapt detections to deep sort input format
                for *xyxy, conf, cls in det:
                    # to deep sort format
                    x_c, y_c, bbox_w, bbox_h = self.xyxy_to_xywh(*xyxy)
                    xywh_obj = [x_c, y_c, bbox_w, bbox_h]
                    xywh_bboxs.append(xywh_obj)
                    confs.append([conf.item()])

                xywhs = torch.Tensor(xywh_bboxs)
                confss = torch.Tensor(confs)
                
                


                for redetect in range(3):
                # pass detections to deepsort
                    outputs = deepsort.update(xywhs, confss, im0)
                
                # draw boxes for visualization
                if len(outputs) > 0:
                    bbox_xyxy = outputs[:, :4]
                    identities = outputs[:, -1]
                    self.draw_boxes(im0, bbox_xyxy, identities)
                    
                    # to MOT format
                    tlwh_bboxs = self.xyxy_to_tlwh(bbox_xyxy)

                    # Write MOT compliant results to file
                    for j, (tlwh_bbox, output) in enumerate(zip(tlwh_bboxs, outputs)):

                        self.checkIfDuplicates(identities)
            else:
                s += '%g %s, ' % (0, 'person')  # add to string
                deepsort.increment_ages()
            people_count = s    
            fps = 1/(t2 - t1)
            unique_id_list_count = len(self.g_unique_id_list)
            self.sv.now_detect_count = unique_id_list_count
            l_log_time = self.sv._log_time
            dtstring = dt.strftime( '%Y-%m-%d %H:%M:%S - ' )
            self.sv.date_string = dtstring
            #self.sv.output_format_string = "{dtstring}目前偵測到 {people_count} (FPS:{fps:.2f})\\n人流 ：{unique_id_list_count} / {l_log_time}秒 "
            self.sv.output_format_string = self.ui.lineEdit_3.text() 
            try:
                logging_out = self.sv.output_format_string.format(**locals())
            except Exception as e :
                print(e)
                logging_out = "輸出格式錯誤，請重新確認"
            if(self._time_counter > self.sv._log_time):
                self._time_counter = 0
                self.total_unique_list_update(self.g_unique_id_list,start_time)
                
                self.sv.avg_detect_count = unique_id_list_count
                
                self.now_log_out = logging_out
                self.g_unique_id_list.clear()
            # Stream results
            if self.sv._show_video_streaming:
                cv2.waitKey(1)
                tmphei =int(im0.shape[0] * (1/self.sv.img_size_zoom))
                tmpwid =int(im0.shape[1] * (1/self.sv.img_size_zoom))
                dim = (tmpwid, tmphei)
                im = cv2.resize(im0,dim)
                cv2.imshow("show", im)                 
            else:
                cv2.destroyAllWindows()
    cv2.destroyAllWindows()
    if(mode == 'yt' or mode == 'cctv'):
        self.g_webdriver.quit()