from PIL import Image
import glob, os
import sys

def flip_image(image_path, saved_location):
    image_obj = Image.open(image_path)
    rotated_image = image_obj.transpose(Image.FLIP_LEFT_RIGHT)
    rotated_image.save(saved_location)

if __name__ == '__main__':

    if len(sys.argv) != 2:
        print ("python mirrorer.py <folder>")
        exit()

    os.chdir(os.getcwd() + "/" + sys.argv[1])
    for file in glob.glob("*.png"):
        image = file
        flip_image(image, file)
