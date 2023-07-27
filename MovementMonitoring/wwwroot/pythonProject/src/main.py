from time import sleep
from threading import Thread

import cv2
import pika

import configuration as cfg
import dataManager as dataMgr
import embeddingsTable as embTable
from faceDetector import FaceDetector
from validator import Validator, InputType


def loop():
    connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
    channel = connection.channel()
    channel.queue_declare(queue='validator')
    i = 1
    validators = []
    face_detectors = {}
    for source in cfg.sources:
        print(source)
        source = next(iter(source.values()))
        print(source)
        res = source['res']
        if res > cfg.max_detector_size:
            res = cfg.max_detector_size
        elif res < cfg.min_detector_size:
            res = cfg.min_detector_size
        if res in face_detectors:
            fd = face_detectors[res]
        else:
            fd = FaceDetector(max_size=res)
            face_detectors[res] = fd
        src_type = source['type']
        match src_type:
            case "CAM":
                src_type = InputType.CAM
                src_cap = cv2.VideoCapture(source['src'])
            case "VIDEO":
                src_type = InputType.VIDEO
                src_cap = cv2.VideoCapture(source['src'])
            case "IMAGE":
                src_type = InputType.IMAGE
                src_cap = cv2.imread(source['src'])
            case _:
                src_cap = ''
        val = Validator(source_id=str(i),
                        source_cap=src_cap,
                        source_type=src_type,
                        face_detector=fd,
                        channel=channel,
                        validation_threshold=cfg.validation_threshold)
        validators.append(val)
        i += 1

    while True:
        for validator in validators:
            validator.validate()
        #embTable.compare_persons()


def uploads():
    while True:
        dataMgr.check_uploads(embTable)
        sleep(3)


if __name__ == '__main__':
    embTable.print_embeddings_table()
    th = Thread(target=loop, args=())
    th2 = Thread(target=uploads, args=())
    th.start()
    th2.start()
