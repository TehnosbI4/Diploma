import datetime
import os
import os.path as osp
import cv2
import numpy

from faceDetector import FaceDetector
from person import PersonData


# noinspection PyBroadException
# noinspection PyDefaultArgument
class DataManager:
    def __init__(self, face_size: list[int], path_to_data: str, events_path: str, cameras_path: str, uploads_path: str,
                 supported_extensions=['.jpg', '.jpeg', '.png']):
        """
        Class for data operations.

        :param face_size: size of the face photo in pixels. For example face_size = [112, 112].
        :param path_to_data: path leading to the folder with guid folders.
        :param events_path: path leading to the folder with events data.
        :param cameras_path: path leading to the folder with cameras last capture.
        :param supported_extensions: list of supported photo extensions. By default ['.jpg', '.jpeg', '.png'].
        """

        self.__face_size = face_size
        self.__check_folder_exist(path_to_data)
        self.__path_to_data = path_to_data
        self.__check_folder_exist(events_path)
        self.__events_path = events_path
        self.__check_folder_exist(cameras_path)
        self.__cameras_path = cameras_path
        self.__check_folder_exist(uploads_path)
        self.__uploads_path = uploads_path
        self.__face_detector_for_align = FaceDetector(face_size, 512)
        self.__supported_extensions = supported_extensions

    def get_aligned_image_by_guid(self, guid: str):
        """
        Returns aligned face images for the given guid.

        :param guid: the guid of the person
        :return: array of PersonData objects
        """

        guid_path = osp.join(self.__path_to_data, guid)
        person_data_array = []
        if osp.isdir(guid_path):
            with os.scandir(guid_path) as files:
                for file in files:
                    file_path = osp.join(guid_path, file.name)
                    person_data = self.get_aligned_image_by_path(file_path)
                    if person_data is not None:
                        person_data_array.append(person_data)
            if len(person_data_array) == 0:
                print(f'Warning> Directory {guid_path} is empty!')
        else:
            print(f'Error> Directory {guid_path} does not exist!')
        return person_data_array

    def get_aligned_image_by_path(self, file_path: str):
        """
        Returns aligned face image for the given path.

        :param file_path: path to image
        :return: PersonData object or None
        """

        image = self.__get_aligned_image_by_path(file_path)
        if image is not None:
            person_data = PersonData(path=file_path)
            person_data.face_image = image
            return person_data
        else:
            return None

    def __get_aligned_image_by_path(self, file_path: str):
        """
        Returns aligned face image for the given path.

        :param file_path: path to image
        :return: aligned image or None
        """

        if osp.isfile(file_path):
            with open(file_path, 'r') as file:
                extension = osp.splitext(file.name)[1]
                if extension in self.__supported_extensions:
                    image = cv2.imread(file_path)
                    if image.shape[0] != self.__face_size[0] or image.shape[1] != self.__face_size[1]:
                        print(f'Warning> Photo {file.name} has wrong size {[image.shape[0], image.shape[1]]}! '
                              f'A face search and a rescaler have been launched. It is recommended to delete '
                              f'the original photo!')
                        face = self.__face_detector_for_align.detect_first_face(image)
                        if face is not None:
                            return face
                        else:
                            print(f'Warning> The detector did not detect a face! The detector may have a too '
                                  f'low max_size value. Check the file {file.name} and make sure that there '
                                  f'is a face image on it!')
                            return None
                    else:
                        return image
                else:
                    print(f'Warning> File {file.name} has an incompatible extension type {extension}, it is '
                          f'recommended to delete it!')
                    return None
        else:
            print(f'Warning> Find folder in directory. Recommended to delete it!')
            return None

    def get_guid_list(self):
        """
        Returns all guid as list.

        :return: all guid as list
        """

        guid_list = []
        with os.scandir(self.__path_to_data) as dirs:
            for dr in dirs:
                if osp.isdir(osp.join(self.__path_to_data, dr.name)):
                    guid_list.append(dr.name)
                else:
                    print(f'Warning> There should be no files in the root folder! '
                          f'It is recommended to delete the file {dr.name}')
        if len(guid_list) == 0:
            print(f'Warning> Root directory is empty!')
        return guid_list

    def write_event(self, guid: str, source_id: str, time: str, image: numpy.ndarray, extension='.png'):
        """
        Writes the photo to the event folder.

        :param source_id: id of camera
        :param time: time mark of capture
        :param guid: the guid of the person
        :param image: face image of guid person
        :param extension: extension in which the file will be written. Default .png

        :return: path of written file or None
        """

        file_path = osp.join(self.__events_path, source_id, time)
        return self.__write_image_to_path(file_path, guid, image, extension)

    def write_image_by_guid(self, guid: str, image: numpy.ndarray, extension='.png'):
        """
        Writes the photo to the guid folder.

        :param guid: the guid of the person
        :param image: face image of guid person
        :param extension: extension in which the file will be written. Default .png

        :return: path of written file or None
        """

        file_path = osp.join(self.__path_to_data, guid)
        return self.__write_image_to_path(file_path, self.get_formatted_datetime(), image, extension)

    def write_camera_capture(self, source_id: str, time: str, capture: numpy.ndarray, extension='.png'):
        """
        Writes the capture to the camera folder.

        :param source_id: id of camera
        :param time: time mark of capture
        :param capture: image of capture
        :param extension: extension in which the file will be written. Default .png

        :return: path of written file or None
        """
        file_path = osp.join(self.__cameras_path, source_id)
        return self.__write_image_to_path(file_path, time, capture, extension)

    def __write_image_to_path(self, path: str, file_name: str, image: numpy.ndarray, extension='.png'):
        """
        Writes the image to the path folder.

        :param path: path where the file will be written
        :param file_name: name of written file
        :param image: face image
        :param extension: extension in which the file will be written. Default .png

        :return: path of written file or None
        """

        if extension in self.__supported_extensions:
            self.__check_folder_exist(path)
            file_path = fr'{path}\{file_name}{extension}'
            cv2.imwrite(file_path, image)
            print(f'Info> Image has been written to path {file_path}.')
            return file_path
        else:
            print(f'Error> Extension {extension} is not supported!')
            return None

    @staticmethod
    def __check_folder_exist(path):
        """
        Create a folder if it does not exist

        :param path: path to folder
        """

        if not osp.exists(path):
            os.makedirs(path)
            print(f'Info> Folder {path} created.')

    @staticmethod
    def get_formatted_datetime():
        return str(datetime.datetime.now().strftime("%Y-%m-%d-%H.%M.%S.%f"))
