from threading import Thread
from time import sleep

import cv2
import pika
import yaml

from dataManager import DataManager
from embedder import Embedder
from embeddingsTable import EmbeddingsTable
from faceDetector import FaceDetector
from validator import Validator, InputType


def val():
    connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
    channel = connection.channel()
    channel.queue_declare(queue='validator')
    val1 = Validator('1', source0, InputType.VIDEO, embedder, emb_table, fd, data_manager, channel)
    val2 = Validator('2', source1, InputType.CAM, embedder, emb_table, fd, data_manager, channel)
    validators = [val1, val2]
    while True:
        data_manager.check_uploads()
        for validator in validators:
            validator.validate()


def ping():
    connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
    channel = connection.channel()
    channel.queue_declare(queue='ping')
    while True:
        # print('>    ping!')
        channel.basic_publish(exchange='', routing_key='ping', body='I ping you!')
        sleep(1)


if __name__ == '__main__':
    with open('../../../../Properties/Config.yml') as f:
        config = yaml.safe_load(f)
        print(config)
        data_path: str = config['п»їdata_path']
        model_path: str = config['model_path']
        events_path: str = config['events_path']
        cameras_path: str = config['cameras_path']
        uploads_path: str = config['uploads_path']
        face_size: list[int] = config['face_size']
        sources = config['sources']
        print(sources)

    data_manager = DataManager(face_size, data_path, events_path, cameras_path, uploads_path)
    embedder = Embedder(face_size, model_path)
    fd = FaceDetector(face_size)
    emb_table = EmbeddingsTable(embedder, data_manager)
    emb_table.print_embeddings_table()
    source0 = cv2.VideoCapture(r'..\data\videos\test_videos\Putin.mp4')
    source1 = cv2.VideoCapture(0)

    th = Thread(target=val, args=())
    th2 = Thread(target=ping, args=())
    th.start()
    th2.start()

    # image1 = cv2.imread(r'C:\Users\TehnsbI4\GitRepos\ConsoleAI\src\AIBlock\data\test_images\nsx.jpg')
    # res = fd.detect_all_faces(image1)
    # res1 = fd.detect_first_face(image1)


# data_path: str = r'C:\Users\amirm\Desktop\Diploma\pythonProject\data\images\test_images'
    # model_path: str = r'C:\Users\amirm\Desktop\Diploma\pythonProject\models\backbone_ir50_ms1m_epoch120.pth'
    # events_path: str = r'C:\Users\amirm\Desktop\Diploma\pythonProject\data\images\events'
    # cameras_path: str = r'C:\Users\amirm\Desktop\Diploma\pythonProject\data\images\cameras'
    # face_size: list[int] = [112, 112]
