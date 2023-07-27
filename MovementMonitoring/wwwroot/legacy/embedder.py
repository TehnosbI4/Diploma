# Original code
# https://github.com/ZhaoJ9014/face.evoLVe.PyTorch/blob/master/util/extract_feature_v1.py
import numpy
import torch
import torch.nn.functional as f
import torchvision.transforms as transforms

from backbone import Backbone


# noinspection PyBroadException
class Embedder:
    def __init__(self, input_size: list[int], model_path: str):
        """
        The class of the embedder object. Calculates embedding.

        :param input_size: size of the input image. For example [112, 112]
        :param model_path: path to arcface model .pth file
        """

        self.__transform = transforms.Compose(
            [
                transforms.ToPILImage(),
                transforms.Resize([int(128 * input_size[0] / 112), int(128 * input_size[0] / 112)], ),
                transforms.CenterCrop([input_size[0], input_size[1]]),
                transforms.ToTensor(),
                transforms.Normalize(mean=[0.5, 0.5, 0.5], std=[0.5, 0.5, 0.5]),
            ],
        )
        self.__device = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')
        self.__backbone = Backbone(input_size)
        self.__backbone.load_state_dict(torch.load(model_path, map_location=torch.device('cpu')))
        self.__backbone.to(self.__device)
        self.__backbone.eval()
        self.__input_size = input_size
        print(f'Info> Embedder with input size {input_size} and device {self.__device} was created')

    def get_input_size(self):
        return self.__input_size

    def get_embedding(self, image: numpy.ndarray):
        """
        Calculate and return embedding.

        :param image: input image in numpy.ndarray format
        """

        try:
            if image.shape[0] == self.__input_size[0] and image.shape[1] == self.__input_size[1]:
                with torch.no_grad():
                    print('Process> embedding calculation started...', sep='', end='')
                    image = self.__transform(image)
                    image = torch.unsqueeze(image, 0)
                    embedding = f.normalize(self.__backbone(image.to(self.__device))).cpu()
                    print(' Done!')
                return embedding
            else:
                print('Error> Incorrect image size!')
                return None
        except Exception:
            print(Exception)
            return None

    def get_embeddings_list(self, images):
        embeddings = []
        try:
            for image in images:
                embeddings.append(self.get_embedding(image))
            return embeddings
        except Exception:
            print(Exception)
            return None
