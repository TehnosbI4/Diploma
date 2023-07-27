import datetime
import os
import os.path as osp

import cv2
import numpy

import configuration as cfg
import embedder as emb
from faceDetector import FaceDetector
from person import PersonData


def get_aligned_faces_by_guid(guid: str):
    """
    Returns aligned face images for the given guid.

    :param guid: the guid of the person
    :return: array of PersonData objects
    """
    __check_folder_exist(cfg.data_path)
    guid_path = osp.join(cfg.data_path, guid)
    person_data_array = []
    if osp.isdir(guid_path):
        with os.scandir(guid_path) as files:
            for file in files:
                file_path = osp.join(guid_path, file.name)
                person_data = get_aligned_image_by_path(file_path)
                if person_data is not None:
                    person_data_array.append(person_data)
        if len(person_data_array) == 0:
            print(f'Warning> Directory {guid_path} is empty!')
    else:
        print(f'Error> Directory {guid_path} does not exist!')
    return person_data_array


def get_aligned_image_by_path(file_path: str):
    """
    Returns aligned face image for the given path.

    :param file_path: path to image
    :return: PersonData object or None
    """

    image = __get_aligned_face_by_path(file_path)
    if image is not None:
        person_data = PersonData(path=file_path)
        person_data.face_image = image
        return person_data
    else:
        return None


def __get_aligned_face_by_path(file_path: str):
    """
    Returns aligned face image for the given path.

    :param file_path: path to image
    :return: aligned face image or None
    """

    if osp.isfile(file_path):
        with open(file_path, 'r') as file:
            extension = osp.splitext(file.name)[1]
            if extension in cfg.supported_extensions:
                image = cv2.imread(file_path)
                if image.shape[0] != cfg.face_size[0] or image.shape[1] != cfg.face_size[1]:
                    print(f'Warning> Photo {file.name} has wrong size {[image.shape[0], image.shape[1]]}! '
                          f'A face search and a rescaler have been launched. It is recommended to delete '
                          f'the original photo!')
                    image_max_size = max(image.shape[0], image.shape[1])
                    if image_max_size < cfg.min_detector_size:
                        image_max_size = cfg.min_detector_size
                    elif image_max_size > cfg.max_detector_size:
                        image_max_size = cfg.max_detector_size
                    fd_align = FaceDetector(max_size=image_max_size)
                    face = fd_align.detect_first_face(image)
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


def get_guid_list():
    """
    Returns all guid as list.

    :return: all guid as list
    """

    guid_list = []
    __check_folder_exist(cfg.data_path)
    with os.scandir(cfg.data_path) as dirs:
        for dr in dirs:
            if osp.isdir(osp.join(cfg.data_path, dr.name)):
                guid_list.append(dr.name)
            else:
                print(f'Warning> There should be no files in the root folder! '
                      f'It is recommended to delete the file {dr.name}')
    if len(guid_list) == 0:
        print(f'Warning> Root directory is empty!')
    return guid_list


def write_event(guid: str, source_id: str, time: str, image: numpy.ndarray, extension='.png'):
    """
    Writes the photo to the event folder.

    :param source_id: id of camera
    :param time: time mark of capture
    :param guid: the guid of the person
    :param image: face image of guid person
    :param extension: extension in which the file will be written. Default .png

    :return: path of written file or None
    """

    file_path = osp.join(cfg.events_path, source_id, time)
    return __write_image_to_path(file_path, guid, image, extension)


def write_image_by_guid(guid: str, image: numpy.ndarray, extension='.png'):
    """
    Writes the photo to the guid folder.

    :param guid: the guid of the person
    :param image: face image of guid person
    :param extension: extension in which the file will be written. Default .png

    :return: path of written file or None
    """

    file_path = osp.join(cfg.data_path, guid)
    return __write_image_to_path(file_path, get_formatted_datetime(), image, extension)


def write_camera_capture(source_id: str, time: str, capture: numpy.ndarray, extension='.jpg'):
    """
    Writes the capture to the camera folder.

    :param source_id: id of camera
    :param time: time mark of capture
    :param capture: image of capture
    :param extension: extension in which the file will be written. Default .png

    :return: path of written file or None
    """
    file_path = osp.join(cfg.cameras_path, source_id)
    return __write_image_to_path(file_path, time, capture, extension)


def __write_image_to_path(path: str, file_name: str, image: numpy.ndarray, extension='.png'):
    """
    Writes the image to the path folder.

    :param path: path where the file will be written
    :param file_name: name of written file
    :param image: face image
    :param extension: extension in which the file will be written. Default .png

    :return: path of written file or None
    """

    if extension in cfg.supported_extensions:
        __check_folder_exist(path)
        file_path = fr'{path}\{file_name}{extension}'
        cv2.imwrite(file_path, image)
        print(f'Info> Image has been written to path {file_path}.')
        return file_path
    else:
        print(f'Error> Extension {extension} is not supported!')
        return None


def __check_folder_exist(path: str):
    """
    Create a folder if it does not exist

    :param path: path to folder
    """

    if not osp.exists(path):
        os.makedirs(path)
        print(f'Info> Folder {path} created.')


def get_formatted_datetime():
    return str(datetime.datetime.now().strftime("%Y-%m-%d-%H.%M.%S.%f"))


def check_uploads(embTable):
    """
    Check images in upload folder.
    """
    __check_folder_exist(cfg.uploads_path)
    with os.scandir(cfg.uploads_path) as dirs:
        for dr in dirs:
            guid_path = osp.join(cfg.uploads_path, dr.name)
            if osp.isdir(guid_path):
                print(f'Info> scan uploads for guid {dr.name}.')
                with os.scandir(guid_path) as files:
                    for file in files:
                        file_path = osp.join(guid_path, file.name)
                        if osp.isfile(file_path):
                            print(">>>>>>>> ", file_path)
                            image = __get_aligned_face_by_path(file_path)
                            if image is not None:
                                image_path = write_image_by_guid(dr.name, image)
                                if image_path is not None:
                                    print(f'Info> find face image for guid {dr.name}.')
                                    person_data = PersonData(path=image_path, face_image=image,
                                                             embedding=emb.get_embedding(image))
                                    embTable.add_person_data(dr.name, person_data)
                            os.remove(file_path)
                        else:
                            os.rmdir(file_path)
                with os.scandir(guid_path) as it:
                    if not any(it):
                        os.rmdir(guid_path)
            else:
                os.remove(guid_path)

