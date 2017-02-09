/*
*******************************************************************************************************
*******************************************************************************************************
*** Author: Nigel Hopkins                                                                           ***
*** email : nigel.hopkins@hotmail.co.uk                                                             ***
*** Copyright : September 2016                                                                      ***
*** Description :  This code is not to be distributed without prior permission from the author.     ***
*******************************************************************************************************
*******************************************************************************************************
*/

//Js to create an area for drag and drop upload for photo gallery

function dragAndDrop ()
{
    Dropzone.options.dropzoneForm =
        {
            init: function () {
                this.on("complete", function (data) {
                    //var res = eval('(' + data.xhr.responseText + ')');
                    var res = JSON.parse(data.xhr.responseText);
                });
            }
        };
}


// creates a function for the photo gallery
function photoGallery() {
    $(document).ready(function () {
        $('.thumbnail').click(function () {
            $('.modal-body').empty();
            var title = $(this).parent('a').attr("title");
            $('.modal-title').html(title);
            $($(this).parents('div').html()).appendTo('.modal-body');
            $('#myModal').modal({ show: true });
        });
    });
};

// Creates a function that detects a change in the window size
function Resize() {
    $(window).on('resize', function () {
        // Queries the window size and executes code if its condition is meet
        if ($(window).width() >= 1024) {
            //changes the display property in the css code to block
            $('#sidebarContent-wrapper').css('display', 'block');
            // If the pervious query is not executed and the condition of the next one is it will execute the code below.
        } else if ($(window).width() < 1024) {
            // changes the display property in the css code to none.
            $('#sidebarContent-wrapper').css('display', 'none');
        }
    });

}
//Creates a function which the name of an div element is passed into
function setElem(elemName) {
    /* 
       creates an object which assigns null values to it's properties.
       This object contains two properties.
    */
    var buttonPress = {
        Name: "",
        Display: ""
    };

    // creates a variable and assigns the emement name of a targeted div element on the webpage
    var elem = document.getElementById(elemName);

    /*
        This is used to support cross browser compatibility
        if the browser will support the first value entered it will execute it code
        eles it will check to see if the other command is supported by the browser.
    */
    if (elem.currentStyle) {
        var displayStyle = elem.currentStyle.display;
    } else if (window.getComputedStyle) {
        var displayStyle = window.getComputedStyle(elem, null).getPropertyValue("display");
    }

    /*
        This is where values are passed into the buttonPress object
    */
    buttonPress.Name = elemName;
    buttonPress.Display = displayStyle;

    // The display function is called and the buttonPress object is passed into the function
    Display(buttonPress);
}
/*
    A function called Display is created 
    This function determans weather the select div element display property is to be set as block(visible) or none(hidden)
    the value button is used to access the properties held within the object buttonPress
*/
function Display(button) {

    switch (button.Display) {
        case "none":
            button.Display = "block";
            break;
        case "block":
            button.Display = "none";
            break;
    }

    document.getElementById(button.Name).style.display = button.Display;
}
onload = function () {

    Resize();
    photoGallery();
    dragAndDrop();
    var PullDownMenu = document.getElementById('PullDownMenu');
    var changeTypeName = document.getElementsByClassName('changeTypeName');
    var adminLogonButton = document.getElementById('adminLogonButton');
    var PasswordReveal = document.getElementById('PasswordReveal');
    var checkAll = document.getElementById('checkAll');
    var selectAll = document.getElementsByClassName('isSelected');
    var btnSubmit = document.getElementById('btnSubmit');

    if (checkAll) {
        checkAll.addEventListener("change", function (e) {
                    for (var i = 0; i < selectAll.length; i++) {
                    selectAll[i].checked = checkAll.checked;
                    }
        });
    }

    if (PullDownMenu)
    {
        PullDownMenu.addEventListener("click", function () {
            setElem("sidebarContent-wrapper");
        });
    }
    if (changeTypeName)
    {
        for (var i = 0; i < changeTypeName.length; i++)
        {
            changeTypeName[i].addEventListener("click", function () {
                setElem("popupWindow");
            });
        }
    }
   
    if (adminLogonButton)
    {
        adminLogonButton.addEventListener("click", function () {
            setElem("adminLoginContainer");
        });
    }
    if (PasswordReveal)
    {
        PasswordReveal.addEventListener("click", function () {


            var checkbox = document.getElementById('PasswordReveal');
            var passwordInput = document.getElementById('password');

            if (checkbox.checked) {
                passwordInput.type = "text";
            }
            else {
                passwordInput.type = "password";
            }

        })
    }

    document.getElementById('')
}

