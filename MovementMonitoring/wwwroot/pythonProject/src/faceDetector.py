import cv2
import numpy as np
from retinaface.pre_trained_models import get_model

import configuration as cfg


# noinspection PyBroadException

class FaceDetector:
    def __init__(self, max_size=2048):
        """
        Face detector class. Detect faces.

        :param max_size: size in pixels on the longest side of the processed image. Default 2048
        """

        self.__model = get_model('resnet50_2020-07-20', max_size=max_size)
        self.__model.eval()
        self.__max_size = max_size
        print(f'Info> Face detector with max size {max_size} and device {cfg.device} was created')

    def detect_all_faces(self, image):
        """
        Return a list of all found faces.

        :param image: input image
        :return: list of all found faces
        """

        try:
            img = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            print('Process> Face search started...', sep='', end='')
            annotation = self.__model.predict_jsons(image=img, confidence_threshold=cfg.confidence_threshold)
            print(' Done!')
            faces = []
            for bb in annotation:
                bb = np.array(bb['bbox'], dtype=np.int32)
                if len(bb) == 4:
                    face = self.__extract_face(image, bb)
                    faces.append(face)
                else:
                    print('Warning> No faces found in the image!')
                    faces = None
            return faces
        except Exception:
            print(Exception)
            return None

    def detect_first_face(self, image):
        """
        Return a first found faces.

        :param image: input image
        :return: first found faces
        """

        try:
            img = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            print('Process> Face search started...', sep='', end='')
            annotation = self.__model.predict_jsons(img)
            print(' Done!')
            bb = np.array(annotation[0]['bbox'], dtype=np.int32)
            if len(bb) == 4:
                face = self.__extract_face(image, bb)
            else:
                print('Warning> No faces found in the image!')
                face = None
            return face
        except Exception:
            print(Exception)
            return None

    @staticmethod
    def __extract_face(image, bb):
        """
        Extracts faces from the image by the coordinates of the bounding box.

        :param image: image from where the face should be extracted
        :param bb: bounding box
        :return: face image
        """

        img = image[bb[1]:bb[3], bb[0]:bb[2]]
        img_width = bb[2] - bb[0]
        img_height = bb[3] - bb[1]
        if img_height >= img_width:
            padding = int((img_height - img_width) / 2)
            if bb[0] - padding >= 0:
                bb0 = bb[0] - padding
                img = image[bb[1]:bb[3], bb0:bb0 + img_height]
            else:
                img = image[bb[1]:bb[3], 0:img_height]
        elif img_height < img_width:
            padding = int((img_width - img_height) / 2)
            if bb[1] - padding >= 0:
                bb1 = bb[1] - padding
                img = image[bb1:bb1 + img_width, bb[0]:bb[2]]
            else:
                img = image[0:img_width, bb[0]:bb[2]]
        img_width = img.shape[0]
        img_height = img.shape[1]
        print(f'Info> Extracted face with bounding box {bb}, width {img_width} and height {img_height}')
        img = cv2.resize(img, (cfg.face_size[0], cfg.face_size[1]))
        return img
